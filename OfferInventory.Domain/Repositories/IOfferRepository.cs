using OfferInventory.Domain.Entities;

namespace OfferInventory.Domain.Repositories;

public interface IOfferRepository
{
    Task AddRangeAsync(IEnumerable<TransportOffer> offers, CancellationToken ct = default);
    Task<List<TransportOffer>> GetAllAsync(CancellationToken ct = default);
}
