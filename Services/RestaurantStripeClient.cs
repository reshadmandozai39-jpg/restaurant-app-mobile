using System.Net.Http.Json;
using System.Text.Json;
using NET_MAUI_App.Services;

namespace NET_MAUI_App.Services;

public sealed record StripeCheckoutSession(string SessionId, Uri CheckoutUrl);

public static class RestaurantStripeClient
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(30) };

    public static async Task<StripeCheckoutSession> CreateCheckoutAsync(
        IReadOnlyList<RestaurantCartLine> cart, int persons, int tableNumber, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            requestId = Guid.NewGuid(),
            persons,
            tableNumber,
            items = cart.Select(line => new { id = line.Item.Id, quantity = line.Quantity }).ToArray()
        };
        using var response = await Client.PostAsJsonAsync(new Uri(GetBaseUri(), "payments/checkout"), request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(ReadError(body, "Stripe-Checkout konnte nicht gestartet werden."));

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;
        if (!root.TryGetProperty("sessionId", out var id) || string.IsNullOrWhiteSpace(id.GetString()) ||
            !root.TryGetProperty("checkoutUrl", out var url) || !Uri.TryCreate(url.GetString(), UriKind.Absolute, out var checkoutUri) || checkoutUri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("Das Backend hat keine gültige Stripe-Checkout-Adresse zurückgegeben.");
        return new StripeCheckoutSession(id.GetString()!, checkoutUri);
    }

    public static async Task<StripePaymentDetails> GetPaymentDetailsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (!sessionId.StartsWith("cs_test_", StringComparison.Ordinal))
            throw new ArgumentException("Ungültige Stripe-Test-Checkout-ID.", nameof(sessionId));

        using var response = await Client.GetAsync(new Uri(GetBaseUri(), $"payments/{Uri.EscapeDataString(sessionId)}/status"), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(ReadError(body, "Der Zahlungsstatus konnte nicht geprüft werden."));

        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;
        if (!root.TryGetProperty("paid", out var paid) || paid.ValueKind is not (JsonValueKind.True or JsonValueKind.False) ||
            !root.TryGetProperty("amount", out var amount) || !amount.TryGetDecimal(out var amountValue) ||
            !root.TryGetProperty("currency", out var currencyElement) || string.IsNullOrWhiteSpace(currencyElement.GetString()) ||
            !root.TryGetProperty("paymentMethod", out var methodElement) || string.IsNullOrWhiteSpace(methodElement.GetString()) ||
            !root.TryGetProperty("createdAt", out var createdElement) || !createdElement.TryGetDateTimeOffset(out var createdAt) ||
            !root.TryGetProperty("tableNumber", out var tableElement) || !tableElement.TryGetInt32(out var tableNumber) ||
            !root.TryGetProperty("persons", out var personsElement) || !personsElement.TryGetInt32(out var persons) ||
            !root.TryGetProperty("lines", out var linesElement) || linesElement.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Das Backend hat unvollständige Zahlungsdetails zurückgegeben.");

        var lines = new List<StripePurchaseLine>();
        foreach (var line in linesElement.EnumerateArray())
        {
            if (!line.TryGetProperty("name", out var nameElement) || string.IsNullOrWhiteSpace(nameElement.GetString()) ||
                !line.TryGetProperty("quantity", out var quantityElement) || !quantityElement.TryGetInt32(out var quantity) || quantity < 1 ||
                !line.TryGetProperty("unitPrice", out var priceElement) || !priceElement.TryGetDecimal(out var unitPrice) || unitPrice < 0)
                throw new InvalidOperationException("Das Backend hat eine ungültige Bestellposition zurückgegeben.");
            lines.Add(new StripePurchaseLine(nameElement.GetString()!, quantity, unitPrice));
        }

        return new StripePaymentDetails(sessionId, paid.GetBoolean(), amountValue, currencyElement.GetString()!,
            methodElement.GetString()!, createdAt.LocalDateTime, tableNumber, persons, lines);
    }

    private static Uri GetBaseUri()
    {
        var configured = Preferences.Default.Get("restaurant_backend_url", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(configured))
        {
            if (!OperatingSystem.IsWindows())
                throw new InvalidOperationException("Das lokale Test-Backend läuft auf dem Entwicklungs-PC. Für Android/iOS muss zuerst eine erreichbare HTTPS-Backend-Adresse eingerichtet werden.");
            configured = "http://127.0.0.1:5187/";
        }
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)))
            throw new InvalidOperationException("Die Backend-Adresse muss HTTPS verwenden. Nur localhost ist für lokale Tests mit HTTP erlaubt.");
        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
    }

    private static string ReadError(string body, string fallback)
    {
        try
        {
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                return detail.GetString()!;
            if (json.RootElement.TryGetProperty("error", out var error) && !string.IsNullOrWhiteSpace(error.GetString()))
                return error.GetString()!;
        }
        catch (JsonException)
        {
            return fallback;
        }
        return fallback;
    }
}
