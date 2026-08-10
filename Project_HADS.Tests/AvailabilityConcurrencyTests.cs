using MySqlConnector;
using Xunit;

namespace Project_HADS.Tests;

public class AvailabilityConcurrencyTests
{
    [Fact(Skip = "Requires a disposable MySQL server configured through HADS_TEST_MYSQL.")]
    [Trait("Category", "Integration")]
    public async Task Atomic_inventory_update_allows_only_one_competing_request()
    {
        var connectionString = Environment.GetEnvironmentVariable("HADS_TEST_MYSQL");
        Assert.False(string.IsNullOrWhiteSpace(connectionString));

        var database = $"hads_test_{Guid.NewGuid():N}";
        await using var admin = new MySqlConnection(connectionString);
        await admin.OpenAsync();
        await ExecuteAsync(admin, $"CREATE DATABASE `{database}`");

        try
        {
            var builder = new MySqlConnectionStringBuilder(connectionString) { Database = database };
            await using (var setup = new MySqlConnection(builder.ConnectionString))
            {
                await setup.OpenAsync();
                await ExecuteAsync(setup, "CREATE TABLE TransportOffers (Id CHAR(36) PRIMARY KEY, SeatsAvailable INT NOT NULL)");
                await ExecuteAsync(setup, "INSERT INTO TransportOffers (Id, SeatsAvailable) VALUES ('00000000-0000-0000-0000-000000000001', 2)");
            }

            async Task<int> CompeteAsync()
            {
                await using var connection = new MySqlConnection(builder.ConnectionString);
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "UPDATE TransportOffers SET SeatsAvailable = SeatsAvailable - 2 WHERE Id = '00000000-0000-0000-0000-000000000001' AND SeatsAvailable >= 2";
                return await command.ExecuteNonQueryAsync();
            }

            var results = await Task.WhenAll(CompeteAsync(), CompeteAsync());
            Assert.Equal(new[] { 0, 1 }, results.OrderBy(x => x).ToArray());

            await using var verify = new MySqlConnection(builder.ConnectionString);
            await verify.OpenAsync();
            await using var count = verify.CreateCommand();
            count.CommandText = "SELECT SeatsAvailable FROM TransportOffers LIMIT 1";
            Assert.Equal(0, Convert.ToInt32(await count.ExecuteScalarAsync()));
        }
        finally
        {
            await ExecuteAsync(admin, $"DROP DATABASE IF EXISTS `{database}`");
        }
    }

    private static async Task ExecuteAsync(MySqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
