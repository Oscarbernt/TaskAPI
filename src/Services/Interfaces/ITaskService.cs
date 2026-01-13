using TaskHub.API.Models.Dto;

namespace TaskHub.API.Services.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponse>> GetAllTasksAsync();
        Task<TaskResponse?> GetTaskByIdAsync(int id);
        Task<TaskResponse> CreateTaskAsync(TaskCreateRequest request);
        Task<TaskResponse?> UpdateTaskAsync(int id, TaskUpdateRequest request);
        Task<bool> DeleteTaskAsync(int id);
    }
}
