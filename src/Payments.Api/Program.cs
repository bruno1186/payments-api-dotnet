using Payments.Domain;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/payments", (AuthorizeRequest request, PaymentService service) =>
{
    try
    {
        var payment = service.Authorize(request);
        return Results.Created($"/payments/{payment.Id}", payment);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapGet("/payments/{id:guid}", (Guid id, PaymentService service) =>
{
    try { return Results.Ok(service.Get(id)); }
    catch (PaymentNotFoundException) { return Results.NotFound(); }
});

app.MapGet("/payments", (PaymentService service) => Results.Ok(service.All()));

app.MapPost("/payments/{id:guid}/capture", (Guid id, PaymentService service) =>
{
    try { return Results.Ok(service.Capture(id)); }
    catch (PaymentNotFoundException) { return Results.NotFound(); }
    catch (InvalidPaymentOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
});

app.MapPost("/payments/{id:guid}/refund", (Guid id, PaymentService service) =>
{
    try { return Results.Ok(service.Refund(id)); }
    catch (PaymentNotFoundException) { return Results.NotFound(); }
    catch (InvalidPaymentOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
});

app.Run();

/// <summary>Exposto para permitir testes de integracao com WebApplicationFactory.</summary>
public partial class Program { }
