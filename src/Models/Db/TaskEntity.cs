using System.ComponentModel.DataAnnotations;

namespace TaskHub.API.Models.Db;

public class TaskEntity {

    public TaskEntity(string title, string description, DateTime dueDate) : base()
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
    }

    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
