using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages;

public class CartPage : ContentPage
{
    private readonly Entry persons = new() { Placeholder = "1 bis 8", Keyboard = Keyboard.Numeric, TextColor = RestaurantUi.Ink, PlaceholderColor = RestaurantUi.Muted, FontSize = 16, BackgroundColor = Colors.White };
    private readonly Entry table = new() { Placeholder = "Tischnummer", Keyboard = Keyboard.Numeric, TextColor = RestaurantUi.Ink, PlaceholderColor = RestaurantUi.Muted, FontSize = 16, BackgroundColor = Colors.White };
    private readonly Label personsError = ErrorLabel();
    private readonly Label tableError = ErrorLabel();
    private readonly Label paymentError = ErrorLabel();
    private readonly Label cartError = ErrorLabel();
    private readonly Label summary = RestaurantUi.Caption(string.Empty);
    private readonly Button twint = new() { Text = "TWINT · Stripe-Test", BackgroundColor = Colors.White, TextColor = RestaurantUi.Ink };
    private readonly Button card = RestaurantUi.PrimaryButton("Kreditkarte · Stripe-Test");
    private ScrollView? detailsView;
    private string payment = string.Empty;
    private bool loading;

    public CartPage()
    {
        Title = "Warenkorb";
        BackgroundColor = RestaurantUi.Background;
        persons.TextChanged += (_, _) => SaveInputs();
        table.TextChanged += (_, _) => SaveInputs();
        card.Clicked += (_, _) => SelectPayment("Kreditkarte");
        twint.Clicked += (_, _) => SelectPayment("TWINT");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            loading = true;
            persons.Text = Preferences.Default.Get("restaurant_persons", "1");
            table.Text = Preferences.Default.Get("restaurant_table", "");
            payment = Preferences.Default.Get("restaurant_payment", "");
            loading = false;
            ShowDetails();
        }
        catch (Exception ex)
        {
            loading = false;
            await DisplayAlertAsync("Warenkorb", $"Die Daten konnten nicht geladen werden: {ex.Message}", "OK");
        }
    }

    private static Label ErrorLabel() => new() { TextColor = Colors.Red, FontSize = 13, IsVisible = false };

    private async void SaveInputs()
    {
        if (loading) return;
        try
        {
            Preferences.Default.Set("restaurant_persons", persons.Text ?? "");
            Preferences.Default.Set("restaurant_table", table.Text ?? "");
            Preferences.Default.Set("restaurant_payment", payment);
        }
        catch (Exception ex) { await DisplayAlertAsync("Speichern", $"Die Angaben konnten nicht gespeichert werden: {ex.Message}", "OK"); }
    }

    private void SelectPayment(string value)
    {
        payment = value;
        UpdatePaymentButtons();
        SaveInputs();
    }

    private void UpdatePaymentButtons()
    {
        twint.BackgroundColor = payment == "TWINT" ? RestaurantUi.Blue : Colors.White;
        twint.TextColor = payment == "TWINT" ? Colors.White : RestaurantUi.Ink;
        card.BackgroundColor = payment == "Kreditkarte" ? RestaurantUi.Blue : Colors.White;
        card.TextColor = payment == "Kreditkarte" ? Colors.White : RestaurantUi.Ink;
    }

    private void ShowDetails()
    {
        var cart = RestaurantOrderService.GetCart();
        summary.Text = $"{cart.Sum(line => line.Quantity)} Menüs · CHF {cart.Sum(line => line.LineTotal):F2}";
        UpdatePaymentButtons();
        if (detailsView is not null)
        {
            Content = detailsView;
            return;
        }
        var paymentButtons = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
        paymentButtons.Add(twint, 0, 0);
        paymentButtons.Add(card, 1, 0);
        var next = RestaurantUi.PrimaryButton("Weiter zur Übersicht");
        next.Clicked += OnContinue;
        var menu = new Button { Text = "Menü ändern", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
        menu.Clicked += async (_, _) => await Shell.Current.GoToAsync("//menu");
        detailsView = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                RestaurantUi.Caption("Schritt 1 von 2: Angaben"),
                RestaurantUi.Heading("Anzahl Personen", 17), persons, personsError,
                RestaurantUi.Heading("Tischnummer", 17), table, tableError,
                RestaurantUi.Caption("Die Tischnummer steht auf deinem Tisch. Die Verfügbarkeit kann noch nicht geprüft werden."),
                RestaurantUi.Heading("Zahlungsart", 17), paymentButtons, paymentError,
                RestaurantUi.Caption("Wähle Karte oder TWINT im Stripe-Checkout. Bargeld ist nicht verfügbar; es werden nur Testzahlungen verarbeitet."),
                RestaurantUi.Card(summary), cartError, next, menu
            }
        });
        Content = detailsView;
    }

    private static bool SetError(Label label, string message, bool invalid)
    {
        label.Text = message;
        label.IsVisible = invalid;
        return invalid;
    }

    private async void OnContinue(object? sender, EventArgs e)
    {
        try
        {
            var invalidPersons = SetError(personsError, "Bitte 1 bis maximal 8 Personen eingeben.", !int.TryParse(persons.Text, out var personCount) || personCount < 1 || personCount > 8);
            var invalidTable = SetError(tableError, "Bitte eine gültige Tischnummer eingeben.", !int.TryParse(table.Text, out var tableNumber) || tableNumber < 1);
            var invalidPayment = SetError(paymentError, "Bitte Karte oder TWINT auswählen.", payment is not ("Kreditkarte" or "TWINT"));
            var cart = RestaurantOrderService.GetCart();
            var invalidCart = SetError(cartError, "Bitte zuerst ein Menü auswählen.", cart.Count == 0);
            if (invalidPersons || invalidTable || invalidPayment || invalidCart) return;
            ShowOverview(personCount, tableNumber, cart);
        }
        catch (Exception ex) { await DisplayAlertAsync("Warenkorb", $"Die Übersicht konnte nicht geladen werden: {ex.Message}", "OK"); }
    }

    private void ShowOverview(int personCount, int tableNumber, IReadOnlyList<RestaurantCartLine> cart)
    {
        var details = new VerticalStackLayout { Spacing = 10 };
        details.Children.Add(RestaurantUi.Caption($"Tisch {tableNumber} · {personCount} Personen"));
        foreach (var line in cart) details.Children.Add(RestaurantUi.Caption($"{line.Quantity} × {line.Item.Name} · CHF {line.LineTotal:F2}"));
        details.Children.Add(RestaurantUi.Caption("Zahlungsart: Karte oder TWINT im Stripe-Checkout auswählen"));
        details.Children.Add(RestaurantUi.Heading($"Total: CHF {cart.Sum(line => line.LineTotal):F2}", 17));
        var stripePay = RestaurantUi.PrimaryButton("Mit Stripe-Testkarte bezahlen");
        stripePay.Clicked += async (_, _) =>
        {
            stripePay.IsEnabled = false;
            try
            {
                var checkout = await RestaurantStripeClient.CreateCheckoutAsync(cart, personCount, tableNumber);
                Preferences.Default.Set("restaurant_stripe_session", checkout.SessionId);
                await Browser.Default.OpenAsync(checkout.CheckoutUrl, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex) { await DisplayAlertAsync("Stripe-Testzahlung", ex.Message, "OK"); }
            finally { stripePay.IsEnabled = true; }
        };
        var checkPayment = new Button { Text = "Zahlung prüfen", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
        checkPayment.Clicked += async (_, _) =>
        {
            checkPayment.IsEnabled = false;
            try
            {
                var sessionId = Preferences.Default.Get("restaurant_stripe_session", string.Empty);
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    await DisplayAlertAsync("Zahlungsstatus", "Starte zuerst die Stripe-Testzahlung.", "OK");
                    return;
                }
                if (!await RestaurantStripeClient.IsPaymentCompleteAsync(sessionId))
                {
                    await DisplayAlertAsync("Zahlungsstatus", "Stripe hat noch keine erfolgreiche Zahlung bestätigt.", "OK");
                    return;
                }
                RestaurantOrderService.Clear();
                Preferences.Default.Remove("restaurant_stripe_session");
                await DisplayAlertAsync("Zahlung bestätigt", "Stripe hat die Testzahlung bestätigt. Es wurde kein echter Betrag belastet.", "Fertig");
                ShowDetails();
            }
            catch (Exception ex) { await DisplayAlertAsync("Zahlungsstatus", ex.Message, "OK"); }
            finally { checkPayment.IsEnabled = true; }
        };
        var save = RestaurantUi.PrimaryButton("Entwurf lokal speichern");
        save.Clicked += async (_, _) =>
        {
            save.IsEnabled = false;
            try
            {
                RestaurantStorage.SaveDraft(personCount, tableNumber, payment, cart);
                await DisplayAlertAsync("Entwurf gespeichert", "Der Entwurf ist nur auf diesem Gerät gespeichert. Er ist nicht bezahlt und wurde nicht an das Restaurant gesendet.", "OK");
                ShowDetails();
            }
            catch (Exception ex) { await DisplayAlertAsync("Speichern", $"Der Entwurf konnte nicht gespeichert werden: {ex.Message}", "OK"); }
            finally { save.IsEnabled = true; }
        };
        var change = new Button { Text = "Angaben ändern", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
        change.Clicked += async (_, _) =>
        {
            try { ShowDetails(); }
            catch (Exception ex) { await DisplayAlertAsync("Warenkorb", $"Die Daten konnten nicht geladen werden: {ex.Message}", "OK"); }
        };
        var edit = new Button { Text = "Menü ändern", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
        edit.Clicked += async (_, _) => await Shell.Current.GoToAsync("//menu");
        Content = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                RestaurantUi.Caption("Schritt 2 von 2: Übersicht"), RestaurantUi.Heading("Stimmt alles?"),
                RestaurantUi.Card(details),
                RestaurantUi.Caption("Stripe Checkout bietet Karte und TWINT an, sofern beide für dein Konto im Testmodus verfügbar sind. Echtes Geld wird nicht belastet."),
                stripePay, checkPayment, save, change, edit
            }
        });
    }
}
