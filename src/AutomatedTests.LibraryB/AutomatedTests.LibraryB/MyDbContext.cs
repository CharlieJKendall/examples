using Microsoft.EntityFrameworkCore;

namespace AutomatedTests.LibraryB;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public DbSet<MyEntity> MyEntities { get; set; } = null!;
}

public class MyEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}