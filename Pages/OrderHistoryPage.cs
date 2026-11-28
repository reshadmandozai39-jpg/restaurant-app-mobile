using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages;

public class OrderHistoryPage : ContentPage
{
    private readonly VerticalStackLayout list = new() { Spacing = 10 };

    public OrderHistoryPage()
    {
        Title = "Gespeicherte Entwürfe";
        RestaurantUi.PageTheme(this);
        Content = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                RestaurantUi.Caption("Der Online-Bestellverlauf ist noch nicht angebunden."),
                RestaurantUi.Heading("Gespeicherte Entwürfe", 18),
                RestaurantUi.Caption("Nur auf diesem Gerät gespeichert. Nicht bezahlt und nicht an das Restaurant gesendet."),
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
            var drafts = RestaurantStorage.GetDrafts();
            if (drafts.Count == 0) list.Children.Add(RestaurantUi.Caption("Noch keine Entwürfe gespeichert."));
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
        catch (Exception ex) { await DisplayAlertAsync("Entwürfe", $"Die Daten konnten nicht geladen werden: {ex.Message}", "OK"); }
    }
}
