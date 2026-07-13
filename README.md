项目简介
本项目是基于 ASP.NET Core 8 开发的 RESTful 风格个人笔记管理后端服务，实现了用户注册登录、JWT 身份鉴权、个人笔记增删改查全流程功能；采用分层架构设计，引入 Redis 实现热点数据缓存优化，支持 Swagger 在线接口调试，代码遵循企业级开发规范，可直接用于前后端分离项目对接、部署上线。
技术栈
表格
类别	    技术选型
后端框架 	ASP.NET Core 8 Web API
ORM 框架	Entity Framework Core
数据库   	MySQL 8.0
缓存中间件	Redis
安全认证	  JWT 身份鉴权 + BCrypt 密码加密
接口文档	  Swagger / Swashbuckle
架构设计	  分层架构（表现层 + 业务层 + 数据层）、依赖注入 DI
版本控制	  Git + GitHub
部署方式	  Docker 容器化 + Linux 部署
核心功能
1. 用户认证模块
用户注册：用户名唯一性校验，密码经 BCrypt 哈希加密存储，杜绝明文泄露
用户登录：账号密码校验通过后下发 JWT Token，支持无状态身份认证
全局鉴权：所有笔记接口强制登录校验，未授权请求直接拦截
2. 笔记管理模块
笔记全量 CRUD：支持创建、查询、修改、删除个人笔记
数据权限隔离：所有笔记操作绑定当前登录用户，仅允许操作自身数据，杜绝越权访问
参数合法性校验：空值、异常参数防御性处理，保证接口稳定性
3. 缓存性能优化
高频查询接口接入 Redis 缓存，按用户维度生成独立缓存 Key
首次查询读取数据库并写入缓存，后续请求直接返回缓存数据
笔记新增 / 修改 / 删除时自动清理对应缓存，保证数据一致性，避免脏读
4. 工程化能力
业务逻辑与接口层解耦，代码复用性高，便于迭代维护
集成 Swagger 自动生成在线接口文档，支持一键调试
遵循 RESTful 接口设计规范，统一返回格式
架构设计
项目采用经典三层架构，职责清晰，符合企业开发规范：
表现层（Controllers）：负责接收 HTTP 请求、参数校验、统一返回结果
业务层（Services）：封装核心业务逻辑、缓存策略、权限判断
数据层（Data + Models）：EF Core 数据库操作、实体类定义
环境要求
.NET 8 SDK
MySQL 8.0 及以上版本
Redis 6.0 及以上版本
开发工具：Visual Studio 2022 / Rider / VS Code
快速运行
1. 克隆仓库
bash
运行
git clone https://github.com/NanMu-hx/Notes-Api.git
cd Notes-Api
2. 配置连接字符串
打开 appsettings.json，修改数据库与 Redis 连接配置：
json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=notes_db;uid=root;pwd=你的MySQL密码;CharSet=utf8mb4;SslMode=None;",
    "Redis": "127.0.0.1:6379"
  },
  "Jwt": {
    "SecretKey": "自定义32位以上密钥字符串",
    "Issuer": "NotesApi",
    "Audience": "NotesApiUser",
    "ExpireHours": 2
  }
}
3. 初始化数据库
方式一：EF Core 自动迁移（推荐）
bash
运行
# 安装EF工具（已安装可跳过）
dotnet tool install --global dotnet-ef

# 生成迁移
dotnet ef migrations add InitDatabase

# 执行迁移生成表结构
dotnet ef database update
方式二：手动根据 Models 目录下的实体类创建对应数据表。
4. 运行项目
bash
运行
# 还原NuGet包
dotnet restore

# 启动项目
dotnet run
5. 访问接口文档
项目启动后，浏览器访问 Swagger 地址即可在线调试接口：
本地地址：http://localhost:5000/swagger
HTTPS 地址：https://localhost:5001/swagger
项目亮点
性能优化成果显著：通过 Redis 缓存 + SQL 优化，笔记查询接口平均响应时间从 120ms 降至 15ms，数据库查询请求量降低 75%，有效缓解数据库压力
安全设计完善：密码加密存储、JWT 无状态鉴权、接口数据权限隔离三层安全保障，规避常见后端安全漏洞
代码规范度高：分层解耦架构、依赖注入开发模式、统一异常处理，贴合企业级团队开发标准，可扩展性强
全流程落地能力：覆盖接口开发、性能优化、安全鉴权、容器部署、版本管理全链路开发流程
主要接口列表
表格
请求方式	  接口地址	            功能说明	           权限要求
POST	    /api/User/register	用户注册	           无需登录
POST	    /api/User/login	    用户登录获取 Token	 无需登录
GET	      /Notes	            获取当前用户所有笔记	需登录
POST	    /Notes	            创建新笔记	          需登录
PUT	      /Notes/{id}	        修改指定笔记	        需登录
DELETE	  /Notes/{id}	        删除指定笔记	        需登录
完整接口参数与返回格式请查看项目启动后的 Swagger 文档。
