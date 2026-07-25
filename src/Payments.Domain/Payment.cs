namespace Payments.Domain;

/// <summary>Estados possiveis de uma transacao de pagamento.</summary>
public enum PaymentStatus
{
    Authorized,
    Captured,
    Refunded,
    Declined
}

/// <summary>Transacao de pagamento (agregado de dominio).</summary>
public sealed record Payment
{
    public required Guid Id { get; init; }
    public required string IdempotencyKey { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string CardLast4 { get; init; }
    public PaymentStatus Status { get; init; } = PaymentStatus.Authorized;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public Payment Capture() =>
        Status == PaymentStatus.Authorized
            ? this with { Status = PaymentStatus.Captured }
            : throw new InvalidPaymentOperationException(
                $"Nao e possivel capturar um pagamento com status {Status}.");

    public Payment Refund() =>
        Status == PaymentStatus.Captured
            ? this with { Status = PaymentStatus.Refunded }
            : throw new InvalidPaymentOperationException(
                $"So e possivel estornar um pagamento capturado (status atual: {Status}).");
}

/// <summary>Erro de regra de negocio ao operar um pagamento.</summary>
public sealed class InvalidPaymentOperationException : Exception
{
    public InvalidPaymentOperationException(string message) : base(message) { }
}
