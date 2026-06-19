using first_net8._0.Data;
using first_net8._0.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace first_net8._0.Controllers
{
    // 加[Authorize]：所有接口必须登录才能访问
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _db;
        //  新增：注入Redis缓存服务
        private readonly IRedisCacheService _redisCache;


        //  新增：构造函数注入RedisCache
        public NotesController(AppDbContext db, IRedisCacheService redisCache)
        {
            _db = db;
            _redisCache = redisCache;
        }

        // 获取当前登录用户的ID（从Token解析）
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        //  新增：生成当前用户的缓存Key（每个用户独立缓存）
        private string GetCacheKey()
        {
            return $"note:list:{GetCurrentUserId()}";
        }

        // 获取自己的所有笔记
        [HttpGet]
        public async Task<IActionResult> GetMyNotes() //  加了 async 和 Task<IActionResult>
        {
            var userId = GetCurrentUserId();
            var cacheKey = GetCacheKey();
            //  新增：尝试从Redis缓存获取数据

            // 1.  第一步：先去Redis查缓存
            var cacheNotes = await _redisCache.GetAsync<List<Note>>(cacheKey);
            if (cacheNotes != null)
            {
                // Redis有数据 → 直接返回，不查数据库
                return Ok("缓存数据：" + cacheNotes);
            }

            // 2. Redis没有 → 查询数据库
            var notes = await _db.Notes.Where(n => n.UserId == userId).ToListAsync(); //  推荐改成 ToListAsync()

            // 3.  第三步：把数据库结果存入Redis，5分钟过期
            await _redisCache.SetAsync(cacheKey, notes);


            // 4. 返回数据
            return Ok(notes);
        }

        // 创建笔记（自动绑定当前用户）
        [HttpPost]
        public async Task<IActionResult> CreateNote(string title, string content)
        {
            var note = new Note
            {
                Title = title,
                Content = content,
                UserId = GetCurrentUserId() // 自动赋值
            };

            _db.Notes.Add(note);
            _db.SaveChanges();
            //  新增：新增笔记后，删除Redis缓存
            await _redisCache.DeleteAsync(GetCacheKey());

            return Ok("创建成功，缓存已清除");
        }

        // ===================== 修改笔记（删除缓存） =====================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, string title, string content)
        {
            var note = _db.Notes.FirstOrDefault(n => n.Id == id && n.UserId == GetCurrentUserId());
            if (note == null) return NotFound();

            note.Title = title;
            note.Content = content;
            _db.SaveChanges();

            //  新增：修改后删除缓存
            await _redisCache.DeleteAsync(GetCacheKey());
            return Ok("修改成功，缓存已清除");
        }
    }
}