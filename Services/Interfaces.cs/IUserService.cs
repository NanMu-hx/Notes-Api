using first_net8._0.Models;

namespace first_net8._0.Services.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">明文密码</param>
        /// <returns>注册成功返回true，用户名重复返回false</returns>
        Task<bool> RegisterAsync(string username, string password);

        /// <summary>
        /// 用户登录，校验账号密码并返回JWT令牌
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>登录成功返回Token，失败返回null</returns>
        Task<string?> LoginAsync(string username, string password);
    }
}