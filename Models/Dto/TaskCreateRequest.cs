using System.ComponentModel.DataAnnotations;

namespace TaskHub.Models.Dto;

public class TaskCreateRequest
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public DateTime DueDate { get; set; }
}