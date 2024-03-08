using api.Models;

namespace api.Interfaces
{
    public interface ICommentRepository
    {
         public Task<List<Comment>> GetAllAsync();
         public Task<Comment> GetByIdAsync(int id);
         public Task<Comment> CreateAsync(Comment commentModel);
         public Task<Comment> DeleteAsync (int id);
    }
}