using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.Models.Db;
using TaskHub.API.Models.Dto;

namespace TaskHub.API.Controllers;

[ApiController]
[Route("api/tasks")]
internal class TaskController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await context.Tasks.ToListAsync();
        return Ok(tasks.Select(t => new TaskResponse
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            DueDate = t.DueDate,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }));
    }
    [HttpPost]
    public async Task<IActionResult> CreateTask(TaskCreateRequest request)
    {
        var task = new TaskEntity(request.Title, request.Description, request.DueDate)
        {
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskUpdateRequest request)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        
        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        
        return Ok(new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();
        return NoContent();
    }   
}