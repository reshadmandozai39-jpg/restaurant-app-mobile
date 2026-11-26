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

    public static async Task<bool> IsPaymentCompleteAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync(new Uri(GetBaseUri(), $"payments/{Uri.EscapeDataString(sessionId)}/status"), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(ReadError(body, "Der Zahlungsstatus konnte nicht geprüft werden."));
        using var json = JsonDocument.Parse(body);
        return json.RootElement.TryGetProperty("paid", out var paid) && paid.ValueKind == JsonValueKind.True;
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
