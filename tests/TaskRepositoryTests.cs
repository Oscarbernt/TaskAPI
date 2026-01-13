using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.Models.Db;
using TaskHub.API.Repositories;

namespace TaskHub.Tests
{
    public class TaskRepositoryTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllTasks()
        {
            using var context = CreateContext(nameof(GetAllAsync_ReturnsAllTasks));
            var task1 = new TaskEntity("Task 1", "Description 1", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var task2 = new TaskEntity("Task 2", "Description 2", DateTime.UtcNow.AddDays(2))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tasks.AddRange(task1, task2);
            await context.SaveChangesAsync();

            var repository = new TaskRepository(context);
            var result = await repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
        {
            using var context = CreateContext(nameof(GetAllAsync_EmptyDatabase_ReturnsEmptyList));
            var repository = new TaskRepository(context);

            var result = await repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdAsync_ExistingTask_ReturnsTask()
        {
            using var context = CreateContext(nameof(GetByIdAsync_ExistingTask_ReturnsTask));
            var task = new TaskEntity("Task", "Description", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var repository = new TaskRepository(context);
            var result = await repository.GetByIdAsync(task.Id);

            Assert.NotNull(result);
            Assert.Equal(task.Id, result.Id);
            Assert.Equal(task.Title, result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingTask_ReturnsNull()
        {
            using var context = CreateContext(nameof(GetByIdAsync_NonExistingTask_ReturnsNull));
            var repository = new TaskRepository(context);

            var result = await repository.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_AddsTaskToDatabase()
        {
            using var context = CreateContext(nameof(AddAsync_AddsTaskToDatabase));
            var repository = new TaskRepository(context);
            var task = new TaskEntity("New Task", "New Description", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            var result = await repository.AddAsync(task);

            Assert.NotNull(result);
            Assert.True(result.Id > 0);

            var savedTask = await context.Tasks.FindAsync(result.Id);
            Assert.NotNull(savedTask);
            Assert.Equal(task.Title, savedTask.Title);
            Assert.Equal(task.Description, savedTask.Description);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingTask()
        {
            using var context = CreateContext(nameof(UpdateAsync_UpdatesExistingTask));
            var task = new TaskEntity("Original", "Original Description", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsCompleted = false
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var repository = new TaskRepository(context);
            task.Title = "Updated";
            task.Description = "Updated Description";
            task.IsCompleted = true;
            task.UpdatedAt = DateTime.UtcNow.AddMinutes(5);

            await repository.UpdateAsync(task);

            var updatedTask = await context.Tasks.FindAsync(task.Id);
            Assert.NotNull(updatedTask);
            Assert.Equal("Updated", updatedTask.Title);
            Assert.Equal("Updated Description", updatedTask.Description);
            Assert.True(updatedTask.IsCompleted);
        }

        [Fact]
        public async Task DeleteAsync_RemovesTaskFromDatabase()
        {
            using var context = CreateContext(nameof(DeleteAsync_RemovesTaskFromDatabase));
            var task = new TaskEntity("To Delete", "Description", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();
            var taskId = task.Id;

            var repository = new TaskRepository(context);
            await repository.DeleteAsync(task);

            var deletedTask = await context.Tasks.FindAsync(taskId);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task ExistsAsync_ExistingTask_ReturnsTrue()
        {
            using var context = CreateContext(nameof(ExistsAsync_ExistingTask_ReturnsTrue));
            var task = new TaskEntity("Task", "Description", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync();

            var repository = new TaskRepository(context);
            var result = await repository.ExistsAsync(task.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistingTask_ReturnsFalse()
        {
            using var context = CreateContext(nameof(ExistsAsync_NonExistingTask_ReturnsFalse));
            var repository = new TaskRepository(context);

            var result = await repository.ExistsAsync(999);

            Assert.False(result);
        }
    }
}
