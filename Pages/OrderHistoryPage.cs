using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages;

public class OrderHistoryPage : ContentPage
{
    private readonly VerticalStackLayout list = new() { Spacing = 10 };

    public OrderHistoryPage()
    {
        Title = "Bestellverlauf";
        RestaurantUi.PageTheme(this);
        Content = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                RestaurantUi.Heading("Bestellverlauf", 18),
                RestaurantUi.Caption("Bezahlte Stripe-Testbestellungen werden live bei Stripe geprüft. Die Referenzen sind nur auf diesem Gerät gespeichert."),
                list
            }
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            list.Children.Clear();
            var sessionIds = await StripePurchaseHistory.GetSessionIdsAsync();
            foreach (var sessionId in sessionIds.Reverse())
            {
                var purchase = await RestaurantStripeClient.GetPaymentDetailsAsync(sessionId);
                if (!purchase.Paid)
                    throw new InvalidOperationException($"Stripe bestätigt die gespeicherte Bestellung {sessionId} derzeit nicht als bezahlt.");

                var header = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                    ColumnSpacing = 12
                };
                header.Add(RestaurantUi.Heading(purchase.CreatedAt.ToString("dd.MM.yyyy HH:mm"), 16));
                header.Add(RestaurantUi.Heading($"{purchase.Amount:F2} {purchase.Currency}", 16), 1);
                var details = new VerticalStackLayout { Spacing = 6 };
                details.Children.Add(header);
                details.Children.Add(RestaurantUi.Caption($"BEZAHLT · {purchase.PaymentMethod} · Tisch {purchase.TableNumber} · {purchase.Persons} Personen"));
                foreach (var line in purchase.Lines)
                    details.Children.Add(RestaurantUi.Caption($"{line.Quantity} × {line.Name} · {line.Total:F2} {purchase.Currency}"));
                list.Children.Add(RestaurantUi.Card(details));
            }

            var drafts = RestaurantStorage.GetDrafts();
            if (sessionIds.Count == 0 && drafts.Count == 0)
                list.Children.Add(RestaurantUi.Caption("Noch keine Bestellungen oder Entwürfe vorhanden."));
            if (drafts.Count > 0)
                list.Children.Add(RestaurantUi.Heading("Lokale Entwürfe", 18));
            foreach (var draft in drafts)
            {
                var header = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                    ColumnSpacing = 12
                };
                header.Add(RestaurantUi.Heading(draft.CreatedAt.ToString("dd.MM.yyyy"), 16));
                header.Add(RestaurantUi.Heading($"CHF {draft.Total:F2}", 16), 1);
                var details = new VerticalStackLayout { Spacing = 6 };
                details.Children.Add(header);
                details.Children.Add(RestaurantUi.Caption($"Unbezahlt · Tisch {draft.TableNumber} · {draft.Persons} Personen · {draft.PaymentMethod}"));
                foreach (var line in draft.Lines)
                    details.Children.Add(RestaurantUi.Caption($"{line.Quantity} × {line.Name} · CHF {line.Price * line.Quantity:F2}"));
                list.Children.Add(RestaurantUi.Card(details));
            }
        }
        catch (Exception ex) { await DisplayAlertAsync("Bestellverlauf", $"Der Verlauf konnte nicht vollständig geladen werden: {ex.Message}", "OK"); }
    }
}
