using Microsoft.EntityFrameworkCore;
using TimeManager.Api.Models;

namespace TimeManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<DailyNote> DailyNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map TaskItem entity to "Tasks" table because class name is different from table name
        modelBuilder.Entity<TaskItem>().ToTable("Tasks");
    }
}
