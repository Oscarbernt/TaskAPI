using System.ComponentModel.DataAnnotations;

namespace TaskHub.API.Models.Dto;

internal class TaskCreateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty; 
    [Required]
    public DateTime DueDate { get; set; }
}