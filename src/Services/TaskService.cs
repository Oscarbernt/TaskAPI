using TaskHub.API.Models.Db;
using TaskHub.API.Models.Dto;
using TaskHub.API.Repositories.Interfaces;
using TaskHub.API.Services.Interfaces;

namespace TaskHub.API.Services
{
    public class TaskService(ITaskRepository repository) : ITaskService
    {
        public async Task<IEnumerable<TaskResponse>> GetAllTasksAsync()
        {
            var tasks = await repository.GetAllAsync();
            return tasks.Select(MapToResponse);
        }

        public async Task<TaskResponse?> GetTaskByIdAsync(int id)
        {
            var task = await repository.GetByIdAsync(id);
            return task == null ? null : MapToResponse(task);
        }

        public async Task<TaskResponse> CreateTaskAsync(TaskCreateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            }

            ValidateTaskFields(request.Title, request.Description, request.DueDate);

            var task = new TaskEntity(request.Title, request.Description, request.DueDate)
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            var createdTask = await repository.AddAsync(task);
            return MapToResponse(createdTask);
        }

        public async Task<TaskResponse?> UpdateTaskAsync(int id, TaskUpdateRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");
            }

            var task = await repository.GetByIdAsync(id);
            if (task == null)
            {
                return null;
            }

            ValidateTaskFields(request.Title, request.Description, request.DueDate);

            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;
            task.IsCompleted = request.IsCompleted;
            task.UpdatedAt = DateTime.UtcNow;

            await repository.UpdateAsync(task);
            return MapToResponse(task);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await repository.GetByIdAsync(id);
            if (task == null)
            {
                return false;
            }

            await repository.DeleteAsync(task);
            return true;
        }

        private static void ValidateTaskFields(string title, string description, DateTime dueDate)
        {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly - Using DTO property names instead of parameter names for client-facing error messages
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.", "Title");
            }
            if (title.Length > 200)
            {
                throw new ArgumentException("Title must not exceed 200 characters.", "Title");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is required.", "Description");
            }
            if (description.Length > 2000)
            {
                throw new ArgumentException("Description must not exceed 2000 characters.", "Description");
            }

            var todayUtc = DateTime.UtcNow.Date;
            if (dueDate.Date < todayUtc)
            {
                throw new ArgumentException("DueDate must be today or a future date (UTC).", "DueDate");
            }
        }

        private static TaskResponse MapToResponse(TaskEntity task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }
    }
}
