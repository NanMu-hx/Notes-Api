using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace first_net8._0.Models  // 必须与你的项目命名空间一致（项目名.文件夹名）
{
    /// <summary>
    /// 笔记实体（对应 MySQL 中的 Notes 表）
    /// </summary>
    public class Note
    {
        /// <summary>
        /// 主键 ID（自增）
        /// </summary>
        [Key]  // 标记为主键
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // 自动生成值（自增）
        public int Id { get; set; }

        /// <summary>
        /// 笔记标题（必填，最大长度100）
        /// </summary>
        [Required]  // 非空约束
        [Column(TypeName = "varchar(100)")]  // MySQL 字段类型
        public string Title { get; set; } = string.Empty;  // 初始化避免空引用

        /// <summary>
        /// 笔记内容（长文本）
        /// </summary>
        [Column(TypeName = "text")]  // MySQL 长文本类型
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间（默认当前时间）
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // 默认值

        /// <summary>
        /// 用户ID（外键，关联用户表）
        /// </summary>
        public int UserId { get; set; }  // 用于多用户场景
        // 导航属性（可选，用于联表查询）
        public User? User { get; set; }
    }
}

