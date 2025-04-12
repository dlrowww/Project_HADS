using Microsoft.EntityFrameworkCore;
using OfferInventory.Application.Services;
using OfferInventory.Domain.Entities;

namespace OfferInventory.Infrastructure.Services
{
    public class OfferService : IOfferService
    {
        private readonly AppDbContext _context;

        public OfferService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransportOffer>> GetAllOffersAsync()
        {
            return await _context.TransportOffers.Include(o => o.Seats).ToListAsync();
        }

        public async Task<TransportOffer> AddOfferAsync(TransportOffer offer)
        {
            _context.TransportOffers.Add(offer);
            await _context.SaveChangesAsync();
            return offer;
        }
    }
}

