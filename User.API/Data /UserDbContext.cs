using Microsoft.EntityFrameworkCore;
using User.API.Models;

namespace User.API.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) {}

    public DbSet<User_repository> Users => Set<User_repository>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User_repository>(e =>
        {
            e.ToTable("Users");
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Username).IsUnique(); // 用户名唯一
            e.Property(u => u.Password).IsRequired();
            e.Property(u => u.CreatedAt).IsRequired();
        });
    }
}
