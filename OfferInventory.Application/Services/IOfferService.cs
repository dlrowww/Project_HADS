using OfferInventory.Domain.Entities;

namespace OfferInventory.Application.Services
{
    public interface IOfferService
    {
        Task<List<TransportOffer>> GetAllOffersAsync();
        Task<TransportOffer> AddOfferAsync(TransportOffer offer);
    }
}

