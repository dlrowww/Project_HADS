using System;
using System.Threading.Tasks;
using Availability.Application.DTOs;

namespace Availability.Application.Services
{
    public interface IAvailabilityService
    {
        Task<AvailabilityDto> GetAvailabilityAsync(Guid offerId);
    }
}
