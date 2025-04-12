using System;

namespace Availability.Domain.Entities
{
    public class AvailabilityRecord
    {
        public Guid OfferId { get; set; }
        public int RemainingSeats { get; set; }
        public bool IsAvailable => RemainingSeats > 0;
    }
}

