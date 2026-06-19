using first_net8._0.Models;  // 引用 Note 模型
using Microsoft.EntityFrameworkCore;  // EF Core 核心命名空间

namespace first_net8._0.Data
{
    /// <summary>
    /// 数据库上下文：EF Core 连接代码与 MySQL 的核心类
    /// </summary>
    public class AppDbContext : DbContext  // 继承自 EF Core 的 DbContext
    {
        // 构造函数（固定写法，用于依赖注入）
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 对应 MySQL 中的 Notes 数据表
        /// 命名规则：复数形式（EF Core 约定优于配置）
        /// </summary>
        public DbSet<Note> Notes { get; set; }  // DbSet<实体> 对应数据库表
        // 新增：用户表
        public DbSet<User> Users { get; set; }   //内存操作入口，EF Core 会根据这个属性创建 Users 表
    }
}