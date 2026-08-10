using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using User.API.Data;  // 确保命名空间正确

public class UserDesignTimeFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        const string cs =
            "server=localhost;port=3307;database=user_db;user=root;password=1234;" +
            "CharSet=utf8mb4;TreatTinyAsBoolean=false";

        var opts = new DbContextOptionsBuilder<UserDbContext>()
            .UseMySql(cs, new MySqlServerVersion(new Version(8,0,42)),
                      o => o.EnableRetryOnFailure())   // 避免瞬时错误
            .Options;

        return new UserDbContext(opts);
    }
}