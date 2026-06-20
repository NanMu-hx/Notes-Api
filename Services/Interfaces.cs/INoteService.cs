using first_net8._0.Models;

namespace first_net8._0.Services.Interfaces
{
    public interface INoteService
    {
        // 查询当前用户所有笔记（带Redis缓存）
        Task<List<Note>> GetMyNotesAsync(int userId);

        // 新增笔记
        Task CreateNoteAsync(Note note);

        // 修改笔记
        Task<bool> UpdateNoteAsync(int id, string title, string content, int userId);

        // 删除笔记
        Task<bool> DeleteNoteAsync(int id, int userId);
    }
}
