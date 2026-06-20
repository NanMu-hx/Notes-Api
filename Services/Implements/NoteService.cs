using first_net8._0.Data;
using first_net8._0.Models;
using first_net8._0.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace first_net8._0.Services.Implements
{
    public class NoteService : INoteService
    {
        private readonly AppDbContext _db;
        private readonly IRedisCacheService _redisCache;

        public NoteService(AppDbContext db, IRedisCacheService redisCache)
        {
            _db = db;
            _redisCache = redisCache;
        }

        // 生成当前用户缓存Key
        private string GetCacheKey(int userId)
        {
            return $"note:list:{userId}";
        }

        public async Task<List<Note>> GetMyNotesAsync(int userId)
        {
            string cacheKey = GetCacheKey(userId);
            // 1. 先查Redis缓存
            List<Note>? cacheNotes = await _redisCache.GetAsync<List<Note>>(cacheKey);
            if (cacheNotes != null)
            {
                return cacheNotes;
            }

            // 2. 查数据库
            List<Note> notes = await _db.Notes.Where(n => n.UserId == userId).ToListAsync();
            // 3. 写入Redis缓存
            await _redisCache.SetAsync(cacheKey, notes);
            return notes;
        }

        public async Task CreateNoteAsync(Note note)
        {
            _db.Notes.Add(note);
            await _db.SaveChangesAsync();
            // 新增后清空当前用户缓存
            string cacheKey = GetCacheKey(note.UserId);
            await _redisCache.DeleteAsync(cacheKey);
        }

        public async Task<bool> UpdateNoteAsync(int id, string title, string content, int userId)
        {
            Note? note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null)
                return false;

            note.Title = title;
            note.Content = content;
            await _db.SaveChangesAsync();

            // 修改后清空缓存
            string cacheKey = GetCacheKey(userId);
            await _redisCache.DeleteAsync(cacheKey);
            return true;
        }

        public async Task<bool> DeleteNoteAsync(int id, int userId)
        {
            Note? note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
            if (note == null)
                return false;

            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();

            // 删除后清空缓存
            string cacheKey = GetCacheKey(userId);
            await _redisCache.DeleteAsync(cacheKey);
            return true;
        }
    }
}