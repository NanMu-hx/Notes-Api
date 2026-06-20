using BCrypt.Net;
using first_net8._0.Data;
using first_net8._0.Models;
using first_net8._0.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace first_net8._0.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        // 构造注入数据库上下文、配置文件
        public UserService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<bool> RegisterAsync(string username, string password)
        {
            // 1. 校验用户名是否已存在
            bool isExist = await _db.Users.AnyAsync(u => u.Username == username);
            if (isExist)
                return false;

            // 2. BCrypt密码加密
            string pwdHash = BCrypt.Net.BCrypt.HashPassword(password);
            User user = new User
            {
                Username = username,
                PasswordHash = pwdHash
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            // 1. 查询用户
            User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return null;

            // 2. 校验密码
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            // 3. 生成JWT Token
            return CreateToken(user);
        }

        // 原Controller里生成Token的私有方法迁移到Service
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(_config["Jwt:ExpireHours"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}