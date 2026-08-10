namespace TimeManager.Api.Models;

public class TaskItem
{
    public int TaskId { get; set; }
    public string Title { get; set; } = null!;
    public string? Note { get; set; }
    public int CategoryId { get; set; }
    public byte Priority { get; set; }
    public DateOnly DeadlineDate { get; set; }
    public int? EstimatedDays { get; set; }
    public bool IsDone { get; set; }
    public bool IsRecurring { get; set; }
    public byte? RecurrenceDayOfWeek { get; set; }
    public DateTime CreatedAt { get; set; }

    public Category Category { get; set; } = null!;
}
