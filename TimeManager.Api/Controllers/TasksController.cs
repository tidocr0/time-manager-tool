using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeManager.Api.Data;
using TimeManager.Api.DTOs;
using TimeManager.Api.Models;

namespace TimeManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/tasks?date=yyyy-MM-dd
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks([FromQuery] DateOnly date)
    {
        return await _context.Tasks
            .Include(t => t.Category)
            .Where(t => (t.StartDate ?? t.DeadlineDate) <= date &&
                        (!t.IsDone || (t.CompletedDate.HasValue && date <= t.CompletedDate.Value)))
            .ToListAsync();
    }

    // GET /api/tasks/week?startDate=yyyy-MM-dd
    [HttpGet("week")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksForWeek([FromQuery] DateOnly startDate)
    {
        var endDate = startDate.AddDays(7);
        return await _context.Tasks
            .Include(t => t.Category)
            .Where(t => t.DeadlineDate >= startDate && t.DeadlineDate <= endDate)
            .ToListAsync();
    }

    // GET /api/tasks/alerts?date=yyyy-MM-dd
    [HttpGet("alerts")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskAlerts([FromQuery] DateOnly date)
    {
        return await _context.Tasks
            .Include(t => t.Category)
            .Where(t => !t.IsDone && (t.StartDate ?? t.DeadlineDate) <= date)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DeadlineDate)
            .ToListAsync();
    }

    // POST /api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItemCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.StartDate.HasValue && dto.StartDate > dto.DeadlineDate)
            return BadRequest("StartDate cannot be after DeadlineDate.");

        if (dto.IsRecurring && dto.RecurrenceDayOfWeek.HasValue)
        {
            var recurringGroupId = Guid.NewGuid();
            var tasks = new List<TaskItem>();
            var baseDeadline = dto.DeadlineDate;
            var baseStart = dto.StartDate;

            for (int i = 0; i < 8; i++)
            {
                var newDeadline = baseDeadline.AddDays(i * 7);
                DateOnly? newStart = null;
                if (baseStart.HasValue)
                {
                    newStart = baseStart.Value.AddDays(i * 7);
                }

                tasks.Add(new TaskItem
                {
                    Title = dto.Title,
                    Note = dto.Note,
                    CategoryId = dto.CategoryId,
                    Priority = dto.Priority,
                    DeadlineDate = newDeadline,
                    StartDate = newStart,
                    DeadlineTime = dto.DeadlineTime,
                    EstimatedDays = dto.EstimatedDays,
                    IsDone = false,
                    IsRecurring = dto.IsRecurring,
                    RecurrenceDayOfWeek = dto.RecurrenceDayOfWeek,
                    RecurringGroupId = recurringGroupId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Tasks.AddRange(tasks);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { date = tasks[0].DeadlineDate }, tasks[0]);
        }
        else
        {
            var task = new TaskItem
            {
                Title = dto.Title,
                Note = dto.Note,
                CategoryId = dto.CategoryId,
                Priority = dto.Priority,
                DeadlineDate = dto.DeadlineDate,
                StartDate = dto.StartDate,
                DeadlineTime = dto.DeadlineTime,
                EstimatedDays = dto.EstimatedDays,
                IsDone = false,
                IsRecurring = dto.IsRecurring,
                RecurrenceDayOfWeek = dto.RecurrenceDayOfWeek,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTasks), new { date = task.DeadlineDate }, task);
        }
    }

    // PUT /api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItemUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (dto.StartDate.HasValue && dto.StartDate > dto.DeadlineDate)
            return BadRequest("StartDate cannot be after DeadlineDate.");

        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        task.Title = dto.Title;
        task.Note = dto.Note;
        task.CategoryId = dto.CategoryId;
        task.Priority = dto.Priority;
        task.DeadlineDate = dto.DeadlineDate;
        task.StartDate = dto.StartDate;
        task.DeadlineTime = dto.DeadlineTime;
        task.EstimatedDays = dto.EstimatedDays;
        task.IsDone = dto.IsDone;
        task.IsRecurring = dto.IsRecurring;
        task.RecurrenceDayOfWeek = dto.RecurrenceDayOfWeek;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /api/tasks/{id}/toggle-done
    [HttpPatch("{id}/toggle-done")]
    public async Task<IActionResult> ToggleTaskDone(int id, [FromQuery] DateOnly? completedDate = null)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        task.IsDone = !task.IsDone;
        if (task.IsDone)
        {
            task.CompletedDate = completedDate ?? DateOnly.FromDateTime(DateTime.Today);
        }
        else
        {
            task.CompletedDate = null;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
