using OfferInventory.Application.Interfaces;
using OfferInventory.Domain.Entities;
using OfferInventory.Domain.Repositories;

namespace OfferInventory.Application.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _repo;
    private readonly Random _rand = new();

    public OfferService(IOfferRepository repo) => _repo = repo;

    public async Task<int> SeedAsync(int count, CancellationToken ct = default)
{
    var list = Enumerable.Range(1, count)
        .Select(_ => {
            var maxSeats = _rand.Next(30, 100); // 随机最大座位数
            var occupiedSeats = _rand.Next(0, maxSeats); // 随机已占用座位
            return new TransportOffer
            {
                FromCity       = $"City{_rand.Next(1,100)}",
                ToCity         = $"City{_rand.Next(1,100)}",
                DepartureTime  = DateTime.UtcNow.AddMinutes(_rand.Next(0,1440)),
                ArrivalTime    = DateTime.UtcNow.AddMinutes(_rand.Next(1441,2880)),
                Price          = (decimal)(_rand.NextDouble() * 100 + 10),
                Currency       = "USD",
                Provider       = "FakeProvider",
                MaxSeats       = maxSeats,
                AvailableSeats = maxSeats - occupiedSeats // 可用座位 = 总座位 - 已占用
            };
        })
        .ToList();

    await _repo.AddRangeAsync(list, ct);
    return list.Count;
}

    public Task<List<TransportOffer>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);
}
