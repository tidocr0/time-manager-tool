using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Api.Data;
using TimeManager.Api.DTOs;
using TimeManager.Api.Models;

namespace TimeManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailyNotesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DailyNotesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{date}")]
    public async Task<ActionResult<DailyNote?>> GetDailyNote(DateOnly date)
    {
        var note = await _context.DailyNotes.FirstOrDefaultAsync(n => n.NoteDate == date);
        return note;
    }

    [HttpPut("{date}")]
    public async Task<IActionResult> UpsertDailyNote(DateOnly date, DailyNoteUpsertDto dto)
    {
        var note = await _context.DailyNotes.FirstOrDefaultAsync(n => n.NoteDate == date);

        if (note == null)
        {
            note = new DailyNote
            {
                NoteDate = date,
                Content = dto.Content
            };
            _context.DailyNotes.Add(note);
        }
        else
        {
            note.Content = dto.Content;
        }

        await _context.SaveChangesAsync();

        return Ok(note);
    }
}
