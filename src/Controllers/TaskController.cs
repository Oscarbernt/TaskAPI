using Microsoft.AspNetCore.Mvc;
using TaskHub.API.Models.Dto;
using TaskHub.API.Services.Interfaces;

namespace TaskHub.API.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        var tasks = await taskService.GetAllTasksAsync();
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask(int id)
    {
        var task = await taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask(TaskCreateRequest request)
    {
        try
        {
            var task = await taskService.CreateTaskAsync(request);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? string.Empty, ex.Message);
            return BadRequest(ModelState);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskUpdateRequest request)
    {
        try
        {
            var task = await taskService.UpdateTaskAsync(id, request);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(ex.ParamName ?? string.Empty, ex.Message);
            return BadRequest(ModelState);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var deleted = await taskService.DeleteTaskAsync(id);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}
