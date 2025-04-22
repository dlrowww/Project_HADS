using Microsoft.EntityFrameworkCore;
using OfferInventory.Domain.Entities;
using OfferInventory.Domain.Repositories;
using OfferInventory.Infrastructure.Data;

namespace OfferInventory.Infrastructure.Repositories;

public class OfferRepository : IOfferRepository
{
    private readonly AppDbContext _db;
    public OfferRepository(AppDbContext db) => _db = db;

    public async Task AddRangeAsync(IEnumerable<TransportOffer> offers, CancellationToken ct = default)
    {
        _db.TransportOffers.AddRange(offers);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<TransportOffer>> GetAllAsync(CancellationToken ct = default)
        => _db.TransportOffers.ToListAsync(ct);
}
