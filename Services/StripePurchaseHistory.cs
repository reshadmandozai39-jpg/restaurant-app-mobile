using System.Text.Json;

namespace NET_MAUI_App.Services;

public sealed record StripePurchaseLine(string Name, int Quantity, decimal UnitPrice)
{
    public decimal Total => Quantity * UnitPrice;
}

public sealed record StripePaymentDetails(
    string SessionId,
    bool Paid,
    decimal Amount,
    string Currency,
    string PaymentMethod,
    DateTime CreatedAt,
    int TableNumber,
    int Persons,
    IReadOnlyList<StripePurchaseLine> Lines);

public static class StripePurchaseHistory
{
    private const string StorageKey = "stripe_paid_checkout_sessions_v1";

    public static async Task<IReadOnlyList<string>> GetSessionIdsAsync()
    {
        var serialized = await SecureStorage.Default.GetAsync(StorageKey);
        if (string.IsNullOrEmpty(serialized)) return Array.Empty<string>();

        var sessionIds = JsonSerializer.Deserialize<List<string>>(serialized)
            ?? throw new InvalidOperationException("Der gespeicherte Stripe-Bestellverlauf ist ungültig.");
        if (sessionIds.Any(id => !id.StartsWith("cs_test_", StringComparison.Ordinal)))
            throw new InvalidOperationException("Der gespeicherte Stripe-Bestellverlauf enthält eine ungültige Checkout-ID.");
        return sessionIds;
    }

    public static async Task AddPaidSessionAsync(string sessionId)
    {
        if (!sessionId.StartsWith("cs_test_", StringComparison.Ordinal))
            throw new ArgumentException("Es können nur Stripe-Test-Checkouts gespeichert werden.", nameof(sessionId));

        var sessionIds = (await GetSessionIdsAsync()).ToList();
        if (sessionIds.Contains(sessionId, StringComparer.Ordinal)) return;

        sessionIds.Add(sessionId);
        await SecureStorage.Default.SetAsync(StorageKey, JsonSerializer.Serialize(sessionIds));
    }
}
