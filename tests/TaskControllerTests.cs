using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Controllers;
using TaskHub.API.Data;
using TaskHub.API.Models.Db;
using TaskHub.API.Models.Dto;

namespace TaskHub.Test
{
    public class TaskControllerTests
    {
        private static ApplicationDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetTasks_ReturnsAllTasks()
        {
            using var context = CreateContext(nameof(GetTasks_ReturnsAllTasks));
            var t1 = new TaskEntity("A", "Desc A", DateTime.UtcNow.AddDays(1)) { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            var t2 = new TaskEntity("B", "Desc B", DateTime.UtcNow.AddDays(2)) { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            context.Tasks.AddRange(t1, t2);
            await context.SaveChangesAsync();

            var controller = new TaskController(context);
            var result = await controller.GetTasks();

            var ok = Assert.IsType<OkObjectResult>(result);
            var items = Assert.IsAssignableFrom<IEnumerable<TaskResponse>>(ok.Value);
            Assert.Equal(2, items.Count());
        }

        [Fact]
        public async Task CreateTask_CreatesAndReturnsCreated()
        {
            using var context = CreateContext(nameof(CreateTask_CreatesAndReturnsCreated));
            var controller = new TaskController(context);

            var request = new TaskCreateRequest
            {
                Title = "New",
                Description = "New Desc",
                DueDate = DateTime.UtcNow.AddDays(3)
            };

            var result = await controller.CreateTask(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var response = Assert.IsType<TaskResponse>(created.Value);
            Assert.Equal(request.Title, response.Title);
            Assert.Equal(request.Description, response.Description);
            Assert.False(response.IsCompleted);
            Assert.True(response.Id > 0);

            var persisted = await context.Tasks.FindAsync(response.Id);
            Assert.NotNull(persisted);
            Assert.Equal(request.Title, persisted!.Title);
            Assert.Equal(request.Description, persisted!.Description);
            Assert.Equal(request.DueDate, persisted!.DueDate);
        }

        [Fact]
        public async Task UpdateTask_Existing_UpdatesAndReturnsOk()
        {
            using var context = CreateContext(nameof(UpdateTask_Existing_UpdatesAndReturnsOk));
            var entity = new TaskEntity("Old", "Old Desc", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsCompleted = false
            };
            context.Tasks.Add(entity);
            await context.SaveChangesAsync();

            var controller = new TaskController(context);
            var request = new TaskUpdateRequest
            {
                Title = "Updated",
                Description = "Updated Desc",
                DueDate = DateTime.UtcNow.AddDays(5),
                IsCompleted = true
            };

            var result = await controller.UpdateTask(entity.Id, request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var resp = Assert.IsType<TaskResponse>(ok.Value);
            Assert.Equal(request.Title, resp.Title);
            Assert.Equal(request.Description, resp.Description);
            Assert.Equal(request.IsCompleted, resp.IsCompleted);

            var updatedEntity = await context.Tasks.FindAsync(entity.Id);
            Assert.NotNull(updatedEntity);
            Assert.Equal(request.Title, updatedEntity!.Title);
            Assert.Equal(request.IsCompleted, updatedEntity.IsCompleted);
            Assert.True(updatedEntity.UpdatedAt >= entity.UpdatedAt);
        }

        [Fact]
        public async Task UpdateTask_NonExisting_ReturnsNotFound()
        {
            using var context = CreateContext(nameof(UpdateTask_NonExisting_ReturnsNotFound));
            var controller = new TaskController(context);
            var request = new TaskUpdateRequest
            {
                Title = "X",
                Description = "X",
                DueDate = DateTime.UtcNow,
                IsCompleted = false
            };

            var result = await controller.UpdateTask(9999, request);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTask_Existing_ReturnsNoContent()
        {
            using var context = CreateContext(nameof(DeleteTask_Existing_ReturnsNoContent));
            var entity = new TaskEntity("ToDelete", "Desc", DateTime.UtcNow.AddDays(1))
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tasks.Add(entity);
            await context.SaveChangesAsync();

            var controller = new TaskController(context);
            var result = await controller.DeleteTask(entity.Id);

            Assert.IsType<NoContentResult>(result);
            var found = await context.Tasks.FindAsync(entity.Id);
            Assert.Null(found);
        }

        [Fact]
        public async Task DeleteTask_NonExisting_ReturnsNotFound()
        {
            using var context = CreateContext(nameof(DeleteTask_NonExisting_ReturnsNotFound));
            var controller = new TaskController(context);

            var result = await controller.DeleteTask(12345);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
