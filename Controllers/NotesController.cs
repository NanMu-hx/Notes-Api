using first_net8._0.Models;
using first_net8._0.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace first_net8._0.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NotesController(INoteService noteService)
        {
            _noteService = noteService;
        }

        // 统一封装：获取当前登录用户ID
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotes()
        {
            int userId = GetCurrentUserId();
            var list = await _noteService.GetMyNotesAsync(userId);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote(string title, string content)
        {
            int userId = GetCurrentUserId();
            Note note = new Note()
            {
                Title = title,
                Content = content,
                UserId = userId
            };
            await _noteService.CreateNoteAsync(note);
            return Ok("创建成功，缓存已清除");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, string title, string content)
        {
            int userId = GetCurrentUserId();
            bool success = await _noteService.UpdateNoteAsync(id, title, content, userId);
            if (!success)
                return NotFound();
            return Ok("修改成功，缓存已清除");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized(new { msg = "用户未授权，请重新登录" });
            }

            bool success = await _noteService.DeleteNoteAsync(id, userId);
            if (!success)
                return NotFound(new { msg = "笔记不存在或无权限删除" });

            return Ok(new { msg = "删除成功" });
        }
    }
}