using System.ComponentModel.DataAnnotations;

namespace TimeManager.Api.DTOs;

public class TaskItemCreateDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required")]
    public string Title { get; set; } = null!;
    public string? Note { get; set; }
    public int CategoryId { get; set; }
    public byte Priority { get; set; }
    public DateOnly DeadlineDate { get; set; }
    public DateOnly? StartDate { get; set; }
    public TimeOnly? DeadlineTime { get; set; }
    public int? EstimatedDays { get; set; }
    public bool IsRecurring { get; set; }
    public byte? RecurrenceDayOfWeek { get; set; }
}
