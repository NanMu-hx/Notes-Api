using first_net8._0;
using first_net8._0.Data;  // 引用 AppDbContext
using first_net8._0.Services.Implements;
using first_net8._0.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;  // EF Core 核心命名空间
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// 1. 注册控制器服务（Web API 必备）
builder.Services.AddControllers();

// 2. 注册 Swagger（接口文档，开发环境用）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 定义 JWT 安全方案
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "请输入 Bearer Token，格式：Bearer {你的Token}"
    });

    // 全局应用安全方案，让所有接口都支持 Bearer Token
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// 3. 注册 EF Core + MySQL 核心配置（最关键！）
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        // 读取 appsettings.json 中的连接字符串
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // 自动检测 MySQL 服务器版本
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);
// 2. 注册Http访问器（获取当前登录用户）// 用于获取JWT中的用户ID
builder.Services.AddHttpContextAccessor();

// 1. 读取Redis连接字符串
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;


// 2. 注册IConnectionMultiplexer为单例（核心：全局唯一连接池，避免连接泄漏）
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
   /* var options = ConfigurationOptions.Parse(redisConnectionString);
    options.ConnectRetry = 3; // 连接重试3次
    options.ConnectTimeout = 5000; // 连接超时5秒
    options.AbortOnConnectFail = false; // 关键：允许 multiplexer 持续重试
    return ConnectionMultiplexer.Connect(options);
*/
    var options = ConfigurationOptions.Parse(redisConnectionString);
    options.AbortOnConnectFail = false;
    var mux = ConnectionMultiplexer.ConnectAsync(options).GetAwaiter().GetResult();
    return mux;
});


// 3. 注册自定义Redis缓存服务（Scoped：每个请求独立实例）
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// 注册业务层服务
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INoteService, NoteService>();


// 3. 配置JWT认证
var jwtKey = builder.Configuration["Jwt:SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

var app = builder.Build();

// 开发环境启用 Swagger（方便调试 API）
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();  // 启用 HTTPS
// 必须按顺序：先认证 → 再授权
app.UseAuthentication();
app.UseAuthorization();     // 启用授权

app.MapControllers();       // 映射控制器路由

app.Run();  // 启动应用