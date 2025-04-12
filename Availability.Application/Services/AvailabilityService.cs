using System;
using System.Threading.Tasks;
using Availability.Application.DTOs;
using Availability.Application.Services;

namespace Availability.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        public Task<AvailabilityDto> GetAvailabilityAsync(Guid offerId)
        {
            // 这里只是演示，实际逻辑可从数据库读取
            var result = new AvailabilityDto
            {
                OfferId = offerId,
                RemainingSeats = 10
            };

            return Task.FromResult(result);
        }
    }
}
