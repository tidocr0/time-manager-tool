namespace TimeManager.Api.Models;

public class DailyNote
{
    public int NoteId { get; set; }
    public DateOnly NoteDate { get; set; }
    public string? Content { get; set; }
}
