namespace TaskHub.Models.Dto;

public class TaskUpdateRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
}