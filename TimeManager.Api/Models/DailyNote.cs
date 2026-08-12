using System.ComponentModel.DataAnnotations;

namespace TimeManager.Api.Models;

public class DailyNote
{
    [Key]
    public int NoteId { get; set; }
    public DateOnly NoteDate { get; set; }
    public string? Content { get; set; }
}
