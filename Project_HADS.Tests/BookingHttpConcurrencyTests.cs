using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MySqlConnector;
using Xunit;

namespace Project_HADS.Tests;

public class BookingHttpConcurrencyTests
{
    [BookingHttpFact]
    [Trait("Category", "EndToEnd")]
    public async Task Two_gateway_booking_requests_cannot_sell_the_same_two_seats()
    {
        var gatewayUrl = Required("HADS_E2E_GATEWAY");
        var token = Required("HADS_E2E_TOKEN");
        var offerId = Guid.Parse(Required("HADS_E2E_OFFER_ID"));
        var mysql = Required("HADS_TEST_MYSQL");

        await using var connection = new MySqlConnection(mysql);
        await connection.OpenAsync();
        var originalSeats = await ReadSeatsAsync(connection, offerId);

        try
        {
            await SetSeatsAsync(connection, offerId, 2);
            using var client = new HttpClient { BaseAddress = new Uri(gatewayUrl) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Task<HttpResponseMessage> BookAsync(string customer) => client.PostAsJsonAsync(
                "/api/booking-proxy",
                new
                {
                    OfferId = offerId,
                    CustomerName = customer,
                    NumberOfSeats = 2,
                    FromCity = "integration-from",
                    ToCity = "integration-to"
                });

            var responses = await Task.WhenAll(BookAsync("concurrent-a"), BookAsync("concurrent-b"));
            var statusCodes = responses.Select(r => r.StatusCode).OrderBy(x => x).ToArray();

            Assert.Equal(new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }.OrderBy(x => x), statusCodes);
            Assert.Equal(0, await ReadSeatsAsync(connection, offerId));
        }
        finally
        {
            await SetSeatsAsync(connection, offerId, originalSeats);
        }
    }

    private static string Required(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"Missing environment variable {name}");

    private static async Task<int> ReadSeatsAsync(MySqlConnection connection, Guid offerId)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT SeatsAvailable FROM offer_inventory.TransportOffers WHERE Id = @offerId";
        command.Parameters.AddWithValue("@offerId", offerId);
        var result = await command.ExecuteScalarAsync();
        return result is null ? throw new InvalidOperationException("Configured offer was not found") : Convert.ToInt32(result);
    }

    private static async Task SetSeatsAsync(MySqlConnection connection, Guid offerId, int seats)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE offer_inventory.TransportOffers SET SeatsAvailable = @seats WHERE Id = @offerId";
        command.Parameters.AddWithValue("@seats", seats);
        command.Parameters.AddWithValue("@offerId", offerId);
        Assert.Equal(1, await command.ExecuteNonQueryAsync());
    }
}

public sealed class BookingHttpFactAttribute : FactAttribute
{
    private static readonly string[] RequiredVariables =
    {
        "HADS_E2E_GATEWAY", "HADS_E2E_TOKEN", "HADS_E2E_OFFER_ID", "HADS_TEST_MYSQL"
    };

    public BookingHttpFactAttribute()
    {
        var missing = RequiredVariables.Where(name =>
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))).ToArray();
        if (missing.Length > 0)
            Skip = $"Set {string.Join(", ", missing)} to run the full HTTP booking concurrency test.";
    }
}
