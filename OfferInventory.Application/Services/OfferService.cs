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
        var now = DateTime.UtcNow.Date.AddDays(1); // 从明天开始造数据

        var list = Enumerable.Range(1, count)
            .Select(_ =>
            {
                var maxSeats = _rand.Next(30, 100);
                var occupiedSeats = _rand.Next(0, maxSeats);

                var depDateTime = now.AddHours(_rand.Next(0, 24)).AddMinutes(_rand.Next(0, 60));
                var arrDateTime = depDateTime.AddHours(_rand.Next(2, 8));

                return new TransportOffer
                {
                    Id = Guid.NewGuid(),
                    Provider = "FakeProvider",
                    Status = "available",
                    TransferType = _rand.Next(0, 2) == 0 ? "Fast" : "Slow",
                    MeansOfTransport = "bus",

                    DepartureDate = DateOnly.FromDateTime(depDateTime),
                    DepartureTime = TimeOnly.FromDateTime(depDateTime),
                    ArrivalDate   = DateOnly.FromDateTime(arrDateTime),
                    ArrivalTime   = TimeOnly.FromDateTime(arrDateTime),

                    FromCity = $"City{_rand.Next(1, 50)}",
                    FromStation = $"Station{_rand.Next(1, 10)}",
                    ToCity = $"City{_rand.Next(51, 100)}",
                    ToStation = $"Station{_rand.Next(11, 20)}",

                    PriceTotal = Math.Round((decimal)(_rand.NextDouble() * 50 + 10), 2),
                    PriceOriginal = 0,
                    PriceAverage = 0,
                    Currency = "USD",

                    SeatsAvailable = maxSeats - occupiedSeats,
                    DurationHours = (arrDateTime - depDateTime).Hours,
                    DurationMinutes = (arrDateTime - depDateTime).Minutes,
                    IntermediateStations = _rand.Next(0, 3),
                    IsPromoted = _rand.Next(0, 2) == 0
                };
            })
            .ToList();

        await _repo.AddRangeAsync(list, ct);
        return list.Count;
    }

    public Task<List<TransportOffer>> GetAllAsync(CancellationToken ct = default)
        => _repo.GetAllAsync(ct);
}
