using Payments.Domain;
using Xunit;

namespace Payments.Tests;

public class PaymentServiceTests
{
    private static AuthorizeRequest ValidRequest(string key = "key-1") =>
        new(key, 100.50m, "BRL", "4111 1111 1111 1111");

    [Fact]
    public void Authorize_ValidRequest_CreatesAuthorizedPayment()
    {
        var service = new PaymentService();
        var payment = service.Authorize(ValidRequest());

        Assert.Equal(PaymentStatus.Authorized, payment.Status);
        Assert.Equal("1111", payment.CardLast4);
        Assert.Equal("BRL", payment.Currency);
    }

    [Fact]
    public void Authorize_SameIdempotencyKey_ReturnsSamePayment()
    {
        var service = new PaymentService();
        var first = service.Authorize(ValidRequest("dup"));
        var second = service.Authorize(ValidRequest("dup"));

        Assert.Equal(first.Id, second.Id);
        Assert.Single(service.All());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Authorize_NonPositiveAmount_Throws(decimal amount)
    {
        var service = new PaymentService();
        var req = new AuthorizeRequest("k", amount, "BRL", "4111111111111111");
        Assert.Throws<ValidationException>(() => service.Authorize(req));
    }

    [Fact]
    public void Authorize_UnsupportedCurrency_Throws()
    {
        var service = new PaymentService();
        var req = new AuthorizeRequest("k", 10m, "JPY", "4111111111111111");
        Assert.Throws<ValidationException>(() => service.Authorize(req));
    }

    [Fact]
    public void Authorize_InvalidCard_Throws()
    {
        var service = new PaymentService();
        var req = new AuthorizeRequest("k", 10m, "BRL", "123");
        Assert.Throws<ValidationException>(() => service.Authorize(req));
    }

    [Fact]
    public void Capture_AuthorizedPayment_MovesToCaptured()
    {
        var service = new PaymentService();
        var payment = service.Authorize(ValidRequest());
        var captured = service.Capture(payment.Id);
        Assert.Equal(PaymentStatus.Captured, captured.Status);
    }

    [Fact]
    public void Refund_RequiresCapturedPayment()
    {
        var service = new PaymentService();
        var payment = service.Authorize(ValidRequest());
        Assert.Throws<InvalidPaymentOperationException>(() => service.Refund(payment.Id));

        service.Capture(payment.Id);
        var refunded = service.Refund(payment.Id);
        Assert.Equal(PaymentStatus.Refunded, refunded.Status);
    }

    [Fact]
    public void Get_UnknownId_Throws()
    {
        var service = new PaymentService();
        Assert.Throws<PaymentNotFoundException>(() => service.Get(Guid.NewGuid()));
    }
}
