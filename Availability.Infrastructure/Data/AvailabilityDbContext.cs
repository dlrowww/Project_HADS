using Microsoft.EntityFrameworkCore;
using Availability.Domain.Entities;

namespace Availability.Infrastructure.Data
{
    public class AvailabilityDbContext : DbContext
    {
        public AvailabilityDbContext(DbContextOptions<AvailabilityDbContext> options) : base(options)
        {
        }

        public DbSet<AvailabilityRecord> AvailabilityRecords { get; set; }
    }
}
