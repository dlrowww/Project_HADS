using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Payment.API.Controllers;
using Payment.Application.DTO;
using Payment.Domain.Enums;
using Payment.Infrastructure;
using Xunit;

namespace Project_HADS.Tests;

public class PaymentTests
{
    [Fact]
    public async Task Development_request_can_exercise_payment_failure()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new PaymentDbContext(options);
        var controller = new PaymentsController(
            db,
            new TestEnvironment { EnvironmentName = Environments.Development },
            new ConfigurationBuilder().AddInMemoryCollection().Build());

        var request = new PaymentRequest
        {
            BookingId = Guid.NewGuid(),
            Amount = 25,
            Currency = "GBP",
            SimulateSuccess = false
        };

        Assert.IsType<OkObjectResult>(await controller.Process(request));
        var payment = await db.Payments.SingleAsync();
        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Project_HADS.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
