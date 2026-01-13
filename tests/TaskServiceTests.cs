using Moq;
using TaskHub.API.Models.Db;
using TaskHub.API.Models.Dto;
using TaskHub.API.Repositories.Interfaces;
using TaskHub.API.Services;

namespace TaskHub.Tests
{
    public class TaskServiceTests
    {
        [Fact]
        public async Task GetAllTasksAsync_ReturnsAllTasks()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var entities = new List<TaskEntity>
            {
                new("Task 1", "Desc 1", DateTime.UtcNow.AddDays(1))
                {
                    Id = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsCompleted = false
                },
                new("Task 2", "Desc 2", DateTime.UtcNow.AddDays(2))
                {
                    Id = 2,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsCompleted = true
                }
            };
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(entities);

            var service = new TaskService(mockRepo.Object);
            var result = await service.GetAllTasksAsync();

            Assert.NotNull(result);
            var tasks = result.ToList();
            Assert.Equal(2, tasks.Count);
            Assert.Equal("Task 1", tasks[0].Title);
            Assert.Equal("Task 2", tasks[1].Title);
        }

        [Fact]
        public async Task GetAllTasksAsync_EmptyRepository_ReturnsEmptyList()
        {
            var mockRepo = new Mock<ITaskRepository>();
            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<TaskEntity>());

            var service = new TaskService(mockRepo.Object);
            var result = await service.GetAllTasksAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ExistingTask_ReturnsTask()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var entity = new TaskEntity("Test Task", "Test Description", DateTime.UtcNow.AddDays(1))
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsCompleted = false
            };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);

            var service = new TaskService(mockRepo.Object);
            var result = await service.GetTaskByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Task", result.Title);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public async Task GetTaskByIdAsync_NonExistingTask_ReturnsNull()
        {
            var mockRepo = new Mock<ITaskRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TaskEntity?)null);

            var service = new TaskService(mockRepo.Object);
            var result = await service.GetTaskByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTaskAsync_ValidRequest_CreatesAndReturnsTask()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var request = new TaskCreateRequest
            {
                Title = "New Task",
                Description = "New Description",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            mockRepo.Setup(r => r.AddAsync(It.IsAny<TaskEntity>()))
                .ReturnsAsync((TaskEntity t) =>
                {
                    t.Id = 1;
                    return t;
                });

            var service = new TaskService(mockRepo.Object);
            var result = await service.CreateTaskAsync(request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            Assert.False(result.IsCompleted);
            mockRepo.Verify(r => r.AddAsync(It.Is<TaskEntity>(t =>
                t.Title == request.Title &&
                t.Description == request.Description &&
                t.IsCompleted == false)), Times.Once);
        }

        [Fact]
        public async Task CreateTaskAsync_NullRequest_ThrowsArgumentNullException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.CreateTaskAsync(null!));
        }

        [Fact]
        public async Task CreateTaskAsync_EmptyTitle_ThrowsArgumentException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);
            var request = new TaskCreateRequest
            {
                Title = "",
                Description = "Valid Description",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(request));
            Assert.Equal("title", exception.ParamName);
            Assert.Contains("Title is required", exception.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_TitleTooLong_ThrowsArgumentException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);
            var request = new TaskCreateRequest
            {
                Title = new string('A', 201),
                Description = "Valid Description",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(request));
            Assert.Equal("title", exception.ParamName);
            Assert.Contains("must not exceed 200 characters", exception.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_EmptyDescription_ThrowsArgumentException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);
            var request = new TaskCreateRequest
            {
                Title = "Valid Title",
                Description = "",
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(request));
            Assert.Equal("description", exception.ParamName);
            Assert.Contains("Description is required", exception.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_DescriptionTooLong_ThrowsArgumentException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);
            var request = new TaskCreateRequest
            {
                Title = "Valid Title",
                Description = new string('A', 2001),
                DueDate = DateTime.UtcNow.AddDays(1)
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(request));
            Assert.Equal("description", exception.ParamName);
            Assert.Contains("must not exceed 2000 characters", exception.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_PastDueDate_ThrowsArgumentException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);
            var request = new TaskCreateRequest
            {
                Title = "Valid Title",
                Description = "Valid Description",
                DueDate = DateTime.UtcNow.AddDays(-1)
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTaskAsync(request));
            Assert.Equal("dueDate", exception.ParamName);
            Assert.Contains("must be today or a future date", exception.Message);
        }

        [Fact]
        public async Task UpdateTaskAsync_ExistingTask_UpdatesAndReturnsTask()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var existingEntity = new TaskEntity("Old Title", "Old Description", DateTime.UtcNow.AddDays(1))
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsCompleted = false
            };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingEntity);
            mockRepo.Setup(r => r.UpdateAsync(It.IsAny<TaskEntity>())).Returns(Task.CompletedTask);

            var service = new TaskService(mockRepo.Object);
            var request = new TaskUpdateRequest
            {
                Title = "Updated Title",
                Description = "Updated Description",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = true
            };

            var result = await service.UpdateTaskAsync(1, request);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(request.Title, result.Title);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.IsCompleted, result.IsCompleted);
            mockRepo.Verify(r => r.UpdateAsync(It.Is<TaskEntity>(t =>
                t.Title == request.Title &&
                t.IsCompleted == true)), Times.Once);
        }

        [Fact]
        public async Task UpdateTaskAsync_NonExistingTask_ReturnsNull()
        {
            var mockRepo = new Mock<ITaskRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TaskEntity?)null);

            var service = new TaskService(mockRepo.Object);
            var request = new TaskUpdateRequest
            {
                Title = "Title",
                Description = "Description",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            var result = await service.UpdateTaskAsync(999, request);

            Assert.Null(result);
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskEntity>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTaskAsync_NullRequest_ThrowsArgumentNullException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var service = new TaskService(mockRepo.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.UpdateTaskAsync(1, null!));
        }

        [Fact]
        public async Task UpdateTaskAsync_InvalidTitle_ThrowsArgumentException()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var existingEntity = new TaskEntity("Old", "Old Desc", DateTime.UtcNow.AddDays(1))
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingEntity);

            var service = new TaskService(mockRepo.Object);
            var request = new TaskUpdateRequest
            {
                Title = "",
                Description = "Valid Description",
                DueDate = DateTime.UtcNow.AddDays(1),
                IsCompleted = false
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateTaskAsync(1, request));
            Assert.Equal("title", exception.ParamName);
        }

        [Fact]
        public async Task DeleteTaskAsync_ExistingTask_DeletesAndReturnsTrue()
        {
            var mockRepo = new Mock<ITaskRepository>();
            var entity = new TaskEntity("To Delete", "Description", DateTime.UtcNow.AddDays(1))
            {
                Id = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(entity);
            mockRepo.Setup(r => r.DeleteAsync(entity)).Returns(Task.CompletedTask);

            var service = new TaskService(mockRepo.Object);
            var result = await service.DeleteTaskAsync(1);

            Assert.True(result);
            mockRepo.Verify(r => r.DeleteAsync(entity), Times.Once);
        }

        [Fact]
        public async Task DeleteTaskAsync_NonExistingTask_ReturnsFalse()
        {
            var mockRepo = new Mock<ITaskRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TaskEntity?)null);

            var service = new TaskService(mockRepo.Object);
            var result = await service.DeleteTaskAsync(999);

            Assert.False(result);
            mockRepo.Verify(r => r.DeleteAsync(It.IsAny<TaskEntity>()), Times.Never);
        }
    }
}
