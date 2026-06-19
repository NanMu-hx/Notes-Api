using BCrypt.Net;
using first_net8._0.Data;
using first_net8._0.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace first_net8._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        // 依赖注入
        public UserController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // 注册接口
        [HttpPost("register")]
        public IActionResult Register(string username, string password)
        {
            // 检查用户名是否已存在
            if (_db.Users.Any(u => u.Username == username))
                return BadRequest("用户名已存在");

            // 密码BCrypt加密
            var pwdHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Username = username,
                PasswordHash = pwdHash
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            return Ok("注册成功");
        }

        // 登录接口 → 返回JWT Token
        [HttpPost("login")]
        public IActionResult Login(string username, string password)
        {
            // 查找用户
            var user = _db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return BadRequest("用户名不存在");

            // 验证密码
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return BadRequest("密码错误");

            // 生成JWT Token
            var token = CreateToken(user);
            return Ok(new { Token = token });
        }

        // 生成JWT方法
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(_config["Jwt:ExpireHours"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}