using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.Models.Db;
using TaskHub.API.Repositories.Interfaces;

namespace TaskHub.API.Repositories
{
    public class TaskRepository(ApplicationDbContext context) : ITaskRepository
    {
        public async Task<IEnumerable<TaskEntity>> GetAllAsync()
        {
            return await context.Tasks.ToListAsync();
        }

        public async Task<TaskEntity?> GetByIdAsync(int id)
        {
            return await context.Tasks.FindAsync(id);
        }

        public async Task<TaskEntity> AddAsync(TaskEntity task)
        {
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            return task;
        }

        public async Task UpdateAsync(TaskEntity task)
        {
            context.Tasks.Update(task);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TaskEntity task)
        {
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await context.Tasks.AnyAsync(t => t.Id == id);
        }
    }
}
