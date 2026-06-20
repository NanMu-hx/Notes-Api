using first_net8._0.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace first_net8._0.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        // 只注入业务接口，不再注入DbContext、IConfiguration
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            bool success = await _userService.RegisterAsync(username, password);
            if (!success)
                return BadRequest("用户名已存在");

            return Ok("注册成功");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            string? token = await _userService.LoginAsync(username, password);
            if (token == null)
                return BadRequest("用户名或密码错误");

            return Ok(new { Token = token });
        }
    }
}