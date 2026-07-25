using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Payments.Domain;
using Xunit;

namespace Payments.Tests;

public class PaymentsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PaymentsApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FullLifecycle_Authorize_Capture_Refund()
    {
        var client = _factory.CreateClient();
        var request = new AuthorizeRequest("api-key-1", 250m, "BRL", "4111111111111111");

        var authorize = await client.PostAsJsonAsync("/payments", request);
        Assert.Equal(HttpStatusCode.Created, authorize.StatusCode);
        var created = await authorize.Content.ReadFromJsonAsync<Payment>();
        Assert.NotNull(created);
        Assert.Equal(PaymentStatus.Authorized, created!.Status);

        var capture = await client.PostAsync($"/payments/{created.Id}/capture", null);
        Assert.Equal(HttpStatusCode.OK, capture.StatusCode);

        var refund = await client.PostAsync($"/payments/{created.Id}/refund", null);
        Assert.Equal(HttpStatusCode.OK, refund.StatusCode);
        var refunded = await refund.Content.ReadFromJsonAsync<Payment>();
        Assert.Equal(PaymentStatus.Refunded, refunded!.Status);
    }

    [Fact]
    public async Task Authorize_InvalidBody_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = new AuthorizeRequest("bad", -5m, "BRL", "4111111111111111");
        var response = await client.PostAsJsonAsync("/payments", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_UnknownPayment_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/payments/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
