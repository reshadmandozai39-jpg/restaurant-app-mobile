using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using NET_MAUI_App.Services;

var builder = WebApplication.CreateBuilder(args);
var secret = builder.Configuration["Stripe:SecretKey"];
if (string.IsNullOrWhiteSpace(secret) || !secret.StartsWith("sk_test_", StringComparison.Ordinal))
    throw new InvalidOperationException("Ein Stripe-Testschlüssel (sk_test_...) muss über Stripe:SecretKey konfiguriert werden. Live-Schlüssel sind nicht erlaubt.");

var publicBaseUrl = builder.Configuration["PublicBaseUrl"];
if (string.IsNullOrWhiteSpace(publicBaseUrl) && builder.Environment.IsDevelopment())
    publicBaseUrl = "http://127.0.0.1:5187";
if (!Uri.TryCreate(publicBaseUrl, UriKind.Absolute, out var publicUri) ||
    (publicUri.Scheme != Uri.UriSchemeHttps && !(builder.Environment.IsDevelopment() && publicUri.IsLoopback)))
    throw new InvalidOperationException("PublicBaseUrl muss eine öffentliche HTTPS-Adresse sein. Nur lokal in Development ist HTTP für localhost erlaubt.");
publicBaseUrl = publicUri.AbsoluteUri.TrimEnd('/');

if (builder.Environment.IsDevelopment())
    builder.WebHost.UseUrls("http://127.0.0.1:5187");
builder.Services.AddHttpClient();
var app = builder.Build();

app.MapPost("/payments/checkout", async (PaymentRequest request, IHttpClientFactory factory, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    if (!Guid.TryParse(request.RequestId, out var requestId) || requestId == Guid.Empty)
        return Results.BadRequest(new { error = "requestId muss eine gültige, nicht leere UUID sein." });
    if (request.Persons is < 1 or > 8 || request.TableNumber < 1)
        return Results.BadRequest(new { error = "Bitte 1 bis 8 Personen und eine gültige Tischnummer angeben." });
    if (request.Items is null || request.Items.Count is < 1 or > 24 || request.Items.Any(item => item is null || string.IsNullOrWhiteSpace(item.Id) || item.Quantity is < 1 or > 20))
        return Results.BadRequest(new { error = "Bitte 1 bis 24 gültige Menüpositionen mit jeweils 1 bis 20 Stück angeben." });
    if (request.Items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != request.Items.Count)
        return Results.BadRequest(new { error = "Jede Menüposition darf nur einmal vorkommen." });

    var form = new Dictionary<string, string>
    {
        ["mode"] = "payment",
        ["allowed_payment_method_types[0]"] = "card",
        ["allowed_payment_method_types[1]"] = "twint",
        ["success_url"] = $"{publicBaseUrl}/payment-return?session_id={{CHECKOUT_SESSION_ID}}",
        ["cancel_url"] = $"{publicBaseUrl}/payment-cancel",
        ["metadata[persons]"] = request.Persons.ToString(CultureInfo.InvariantCulture),
        ["metadata[table_number]"] = request.TableNumber.ToString(CultureInfo.InvariantCulture)
    };
    decimal total = 0;
    for (var index = 0; index < request.Items.Count; index++)
    {
        var line = request.Items[index];
        var item = RestaurantCatalog.Menu.FirstOrDefault(menuItem => menuItem.Id == line.Id);
        if (item is null)
            return Results.BadRequest(new { error = "Eine Menü-ID ist unbekannt." });
        total += item.Price * line.Quantity;
        form[$"line_items[{index}][price_data][currency]"] = "chf";
        form[$"line_items[{index}][price_data][unit_amount]"] = decimal.ToInt64(item.Price * 100).ToString(CultureInfo.InvariantCulture);
        form[$"line_items[{index}][price_data][product_data][name]"] = item.Name;
        form[$"line_items[{index}][quantity]"] = line.Quantity.ToString(CultureInfo.InvariantCulture);
    }

    using var message = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions");
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
    message.Headers.Add("Idempotency-Key", requestId.ToString("D"));
    message.Content = new FormUrlEncodedContent(form);
    using var response = await factory.CreateClient().SendAsync(message, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        var stripeError = await ReadStripeErrorAsync(response, cancellationToken);
        logger.LogWarning("Stripe hat die Checkout-Anfrage mit HTTP {Status} abgelehnt: {StripeError}", (int)response.StatusCode, stripeError);
        return Results.Problem($"Stripe lehnt die Checkout-Anfrage ab: {stripeError}", statusCode: 502);
    }

    using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
    if (!json.RootElement.TryGetProperty("id", out var id) || id.GetString() is not { Length: > 0 } sessionId ||
        !json.RootElement.TryGetProperty("url", out var url) || url.GetString() is not { Length: > 0 } checkoutUrl)
        return Results.Problem("Stripe hat eine unvollständige Checkout-Antwort geliefert.", statusCode: 502);

    return Results.Ok(new { sessionId, checkoutUrl, amount = total, currency = "CHF" });
});

app.MapGet("/payments/{sessionId}/status", async (string sessionId, IHttpClientFactory factory, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    if (!sessionId.StartsWith("cs_test_", StringComparison.Ordinal))
        return Results.BadRequest(new { error = "Ungültige Test-Checkout-ID." });
    using var message = new HttpRequestMessage(HttpMethod.Get, $"https://api.stripe.com/v1/checkout/sessions/{Uri.EscapeDataString(sessionId)}");
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secret);
    using var response = await factory.CreateClient().SendAsync(message, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
        logger.LogWarning("Stripe-Statusabfrage wurde mit HTTP {Status} abgelehnt.", (int)response.StatusCode);
        return Results.Problem("Der Zahlungsstatus konnte bei Stripe nicht geprüft werden.", statusCode: 502);
    }
    using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
    var paid = json.RootElement.TryGetProperty("payment_status", out var status) && status.GetString() == "paid";
    return Results.Ok(new { paid, status = paid ? "paid" : "unpaid" });
});

app.MapGet("/payment-return", () => Results.Content("<!doctype html><html lang=de><meta charset=utf-8><title>Zahlung prüfen</title><body><h1>Zurück zur App</h1><p>Kehre zur Restaurant-App zurück und tippe auf „Zahlung prüfen“. Erst die Bestätigung von Stripe zeigt den Zahlungsstatus.</p></body></html>", "text/html; charset=utf-8"));
app.MapGet("/payment-cancel", () => Results.Content("<!doctype html><html lang=de><meta charset=utf-8><title>Zahlung abgebrochen</title><body><h1>Zahlung abgebrochen</h1><p>Du kannst zur App zurückkehren. Es wurde keine Zahlung bestätigt.</p></body></html>", "text/html; charset=utf-8"));

app.Run();

static async Task<string> ReadStripeErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
{
    try
    {
        using var json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);
        if (json.RootElement.TryGetProperty("error", out var error) &&
            error.TryGetProperty("message", out var message) &&
            !string.IsNullOrWhiteSpace(message.GetString()))
            return message.GetString()!;
    }
    catch (JsonException)
    {
        return "Stripe hat eine unlesbare Fehlerantwort zurückgegeben.";
    }

    return "Stripe hat keinen genaueren Fehlergrund geliefert.";
}

public sealed record PaymentItem(string Id, int Quantity);
public sealed record PaymentRequest(string RequestId, int Persons, int TableNumber, List<PaymentItem> Items);
