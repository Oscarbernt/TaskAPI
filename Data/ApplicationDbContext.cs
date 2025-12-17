using Microsoft.EntityFrameworkCore;
using TaskHub.Models;

namespace TaskHub.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; }

}

