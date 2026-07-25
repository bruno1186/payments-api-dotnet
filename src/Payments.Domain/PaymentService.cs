using System.Collections.Concurrent;

namespace Payments.Domain;

/// <summary>Requisicao de autorizacao de pagamento.</summary>
public sealed record AuthorizeRequest(
    string IdempotencyKey,
    decimal Amount,
    string Currency,
    string CardNumber);

public sealed class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public sealed class PaymentNotFoundException : Exception
{
    public PaymentNotFoundException(Guid id) : base($"Pagamento {id} nao encontrado.") { }
}

/// <summary>
/// Servico de pagamentos com repositorio em memoria e idempotencia por chave.
/// Regras: valor positivo, moeda suportada e cartao com 13-19 digitos.
/// </summary>
public sealed class PaymentService
{
    private static readonly HashSet<string> SupportedCurrencies = new() { "BRL", "USD", "EUR" };

    private readonly ConcurrentDictionary<Guid, Payment> _byId = new();
    private readonly ConcurrentDictionary<string, Guid> _byIdempotencyKey = new();

    public Payment Authorize(AuthorizeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ValidationException("IdempotencyKey e obrigatoria.");

        if (_byIdempotencyKey.TryGetValue(request.IdempotencyKey, out var existingId))
            return _byId[existingId];

        if (request.Amount <= 0m)
            throw new ValidationException("Amount deve ser positivo.");

        var currency = (request.Currency ?? string.Empty).ToUpperInvariant();
        if (!SupportedCurrencies.Contains(currency))
            throw new ValidationException($"Moeda nao suportada: {request.Currency}.");

        var digits = new string((request.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length is < 13 or > 19)
            throw new ValidationException("Numero de cartao invalido.");

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = request.IdempotencyKey,
            Amount = request.Amount,
            Currency = currency,
            CardLast4 = digits[^4..],
        };

        _byId[payment.Id] = payment;
        _byIdempotencyKey[request.IdempotencyKey] = payment.Id;
        return payment;
    }

    public Payment Get(Guid id) =>
        _byId.TryGetValue(id, out var p) ? p : throw new PaymentNotFoundException(id);

    public Payment Capture(Guid id)
    {
        var updated = Get(id).Capture();
        _byId[id] = updated;
        return updated;
    }

    public Payment Refund(Guid id)
    {
        var updated = Get(id).Refund();
        _byId[id] = updated;
        return updated;
    }

    public IReadOnlyCollection<Payment> All() => _byId.Values.ToList();
}
