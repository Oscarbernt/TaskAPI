using TaskHub.API.Models.Db;

namespace TaskHub.API.Repositories.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskEntity>> GetAllAsync();
        Task<TaskEntity?> GetByIdAsync(int id);
        Task<TaskEntity> AddAsync(TaskEntity task);
        Task UpdateAsync(TaskEntity task);
        Task DeleteAsync(TaskEntity task);
        Task<bool> ExistsAsync(int id);
    }
}
