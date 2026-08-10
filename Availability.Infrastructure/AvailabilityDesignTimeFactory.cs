using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Availability.Domain.Enums;


namespace Availability.Infrastructure
{
    public class AvailabilityDesignTimeFactory : IDesignTimeDbContextFactory<AvailabilityDbContext>
    {
        public AvailabilityDbContext CreateDbContext(string[] args)
        {
            // 你可以把连接串硬编码，也可以读取环境变量
            const string cs = "server=localhost;port=3306;database=availability_db;user=root;password=1234";

            var opts = new DbContextOptionsBuilder<AvailabilityDbContext>()
                .UseMySql(cs, new MySqlServerVersion(new Version(8,0,30)))
                .Options;

            return new AvailabilityDbContext(opts);
        }
    }
}

