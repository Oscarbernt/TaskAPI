using Microsoft.EntityFrameworkCore;
using TaskHub.API.Models.Db;

namespace TaskHub.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks { get; set; }

}

