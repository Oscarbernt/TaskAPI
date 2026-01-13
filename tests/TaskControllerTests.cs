using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskHub.API.Controllers;
using TaskHub.API.Models.Dto;
using TaskHub.API.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TaskHub.Test
{
    public class TaskControllerTests
    {
        [Fact]
        public async Task GetTasks_ReturnsAllTasks()
        {
            var mockService = new Mock<ITaskService>();
            var tasks = new List<TaskResponse>
            {
                new() { Id = 1, Title = "A", Description = "Desc A", DueDate = DateTime.UtcNow.AddDays(1), IsCompleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 2, Title = "B", Description = "Desc B", DueDate = DateTime.UtcNow.AddDays(2), IsCompleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };
            mockService.Setup(s => s.GetAllTasksAsync()).ReturnsAsync(tasks);

            var controller = new TaskController(mockService.Object);
            var result = await controller.GetTasks();

            var ok = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<TaskResponse>>(ok.Value);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task GetTask_ExistingId_ReturnsTask()
        {
            var mockService = new Mock<ITaskService>();
            var task = new TaskResponse 
            { 
                Id = 1, 
                Title = "Test", 
                Description = "Test Desc", 
                DueDate = DateTime.UtcNow.AddDays(1), 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            };
            mockService.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(task);

            var controller = new TaskController(mockService.Object);
            var result = await controller.GetTask(1);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TaskResponse>(ok.Value);
            Assert.Equal(1, response.Id);
            Assert.Equal("Test", response.Title);
        }

        [Fact]
        public async Task GetTask_NonExistingId_ReturnsNotFound()
        {
            var mockService = new Mock<ITaskService>();
            mockService.Setup(s => s.GetTaskByIdAsync(999)).ReturnsAsync((TaskResponse?)null);

            var controller = new TaskController(mockService.Object);
            var result = await controller.GetTask(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateTask_ValidRequest_ReturnsCreated()
        {
            var mockService = new Mock<ITaskService>();
            var request = new TaskCreateRequest
            {
                Title = "New",
                Description = "New Desc",
                DueDate = DateTime.UtcNow.AddDays(3)
            };
            var createdTask = new TaskResponse
            {
                Id = 1,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            mockService.Setup(s => s.CreateTaskAsync(request)).ReturnsAsync(createdTask);

            var controller = new TaskController(mockService.Object);
            var result = await controller.CreateTask(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(TaskController.GetTask), created.ActionName);
            var response = Assert.IsType<TaskResponse>(created.Value);
            Assert.Equal(request.Title, response.Title);
            Assert.Equal(request.Description, response.Description);
            Assert.False(response.IsCompleted);
            Assert.Equal(1, response.Id);
        }

        [Fact]
        public async Task CreateTask_InvalidTitle_ReturnsBadRequest()
        {
            var mockService = new Mock<ITaskService>();
            var request = new TaskCreateRequest
            {
                Title = "",
                Description = "Valid Desc",
                DueDate = DateTime.UtcNow.AddDays(1)
            };
            mockService.Setup(s => s.CreateTaskAsync(request))
                .ThrowsAsync(new ArgumentException("Title is required.", nameof(request.Title)));

            var controller = new TaskController(mockService.Object);
            var result = await controller.CreateTask(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(bad.Value);
            Assert.True(modelState.ContainsKey("Title"));
        }

        [Fact]
        public async Task CreateTask_InvalidDueDate_ReturnsBadRequest()
        {
            var mockService = new Mock<ITaskService>();
            var request = new TaskCreateRequest
            {
                Title = "Valid",
                Description = "Valid Desc",
                DueDate = DateTime.UtcNow.AddDays(-1)
            };
            mockService.Setup(s => s.CreateTaskAsync(request))
                .ThrowsAsync(new ArgumentException("DueDate must be today or a future date (UTC).", nameof(request.DueDate)));

            var controller = new TaskController(mockService.Object);
            var result = await controller.CreateTask(request);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(bad.Value);
            Assert.True(modelState.ContainsKey("DueDate"));
        }

        [Fact]
        public async Task UpdateTask_ExistingTask_ReturnsOk()
        {
            var mockService = new Mock<ITaskService>();
            var request = new TaskUpdateRequest
            {
                Title = "Updated",
                Description = "Updated Desc",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = true
            };
            var updatedTask = new TaskResponse
            {
                Id = 1,
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                IsCompleted = request.IsCompleted,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };
            mockService.Setup(s => s.UpdateTaskAsync(1, request)).ReturnsAsync(updatedTask);

            var controller = new TaskController(mockService.Object);
            var result = await controller.UpdateTask(1, request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var resp = Assert.IsType<TaskResponse>(ok.Value);
            Assert.Equal(request.Title, resp.Title);
            Assert.Equal(request.Description, resp.Description);
            Assert.Equal(request.IsCompleted, resp.IsCompleted);
        }

        [Fact]
        public async Task UpdateTask_NonExistingTask_ReturnsNotFound()
        {
            var mockService = new Mock<ITaskService>();
            var request = new TaskUpdateRequest
            {
                Title = "X",
                Description = "X",
                DueDate = DateTime.UtcNow,
                IsCompleted = false
            };
            mockService.Setup(s => s.UpdateTaskAsync(9999, request)).ReturnsAsync((TaskResponse?)null);

            var controller = new TaskController(mockService.Object);
            var result = await controller.UpdateTask(9999, request);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateTask_InvalidDueDate_ReturnsBadRequest()
        {
            var mockService = new Mock<ITaskService>();
            var request = new TaskUpdateRequest
            {
                Title = "Updated",
                Description = "Updated Desc",
                DueDate = DateTime.UtcNow.AddDays(-2),
                IsCompleted = false
            };
            mockService.Setup(s => s.UpdateTaskAsync(1, request))
                .ThrowsAsync(new ArgumentException("DueDate must be today or a future date (UTC).", nameof(request.DueDate)));

            var controller = new TaskController(mockService.Object);
            var result = await controller.UpdateTask(1, request);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(bad.Value);
            Assert.True(modelState.ContainsKey("DueDate"));
        }

        [Fact]
        public async Task DeleteTask_ExistingTask_ReturnsNoContent()
        {
            var mockService = new Mock<ITaskService>();
            mockService.Setup(s => s.DeleteTaskAsync(1)).ReturnsAsync(true);

            var controller = new TaskController(mockService.Object);
            var result = await controller.DeleteTask(1);

            Assert.IsType<NoContentResult>(result);
            mockService.Verify(s => s.DeleteTaskAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteTask_NonExistingTask_ReturnsNotFound()
        {
            var mockService = new Mock<ITaskService>();
            mockService.Setup(s => s.DeleteTaskAsync(12345)).ReturnsAsync(false);

            var controller = new TaskController(mockService.Object);
            var result = await controller.DeleteTask(12345);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
