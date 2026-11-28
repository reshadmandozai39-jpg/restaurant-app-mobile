using System.Text.Json;
using Microsoft.Maui.Controls.Shapes;
using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages;

public class CartPage : ContentPage
{
    private static readonly Color FieldStroke = Color.FromArgb("#CBD5E1");
    private static readonly Color ErrorColor = Color.FromArgb("#B91C1C");
    private readonly Entry persons = new() { Placeholder = "1 bis 8", Keyboard = Keyboard.Numeric, TextColor = RestaurantUi.Ink, PlaceholderColor = RestaurantUi.Muted, FontSize = 16, BackgroundColor = Colors.Transparent, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Entry table = new() { Placeholder = "Tischnummer", Keyboard = Keyboard.Numeric, TextColor = RestaurantUi.Ink, PlaceholderColor = RestaurantUi.Muted, FontSize = 16, BackgroundColor = Colors.Transparent };
    private readonly Label personsError = ErrorLabel();
    private readonly Label tableError = ErrorLabel();
    private readonly Label paymentError = ErrorLabel();
    private readonly Label cartError = ErrorLabel();
    private readonly Label menuCount = new() { TextColor = RestaurantUi.Ink, FontSize = 15 };
    private readonly Label total = RestaurantUi.Heading(string.Empty, 15);
    private readonly Button twint = new() { Text = "TWINT" };
    private readonly Button card = new() { Text = "Kreditkarte" };
    private readonly Button pendingPayment = new() { Text = "Offene Zahlung prüfen", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
    private readonly Border personsBorder;
    private readonly Border tableBorder;
    private readonly Border summaryBorder;
    private ScrollView? detailsView;
    private string payment = "TWINT";
    private bool loading;
    private bool keepOverviewOnAppearing;

    public CartPage()
    {
        Title = "Warenkorb";
        RestaurantUi.PageTheme(this);
        persons.SetAppThemeColor(Entry.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        table.SetAppThemeColor(Entry.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        RestaurantUi.TextTheme(menuCount);
        personsBorder = InputBorder(persons);
        tableBorder = InputBorder(table);
#if ANDROID
        persons.HandlerChanged += RemoveAndroidUnderline;
        table.HandlerChanged += RemoveAndroidUnderline;
#endif
        var summaryGrid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 10
        };
        summaryGrid.Add(new Label { Text = FluentUI.cart_24_regular, FontFamily = FluentUI.FontFamily, FontSize = 22, TextColor = RestaurantUi.Muted, VerticalOptions = LayoutOptions.Center });
        summaryGrid.Add(menuCount, 1);
        summaryGrid.Add(total, 2);
        summaryBorder = RestaurantUi.Card(summaryGrid);
        summaryBorder.SetAppThemeColor(VisualElement.BackgroundColorProperty, RestaurantUi.Background, RestaurantUi.DarkSurface);
        summaryBorder.Padding = 12;
        persons.TextChanged += (_, e) =>
        {
            if (KeepDigitsOnly(persons, e.NewTextValue)) return;
            if (!loading) ValidatePersons();
            SaveInputs();
        };
        table.TextChanged += (_, e) =>
        {
            if (KeepDigitsOnly(table, e.NewTextValue)) return;
            if (!loading && tableError.IsVisible) ValidateTable();
            SaveInputs();
        };
        card.Clicked += (_, _) => SelectPayment("Kreditkarte");
        twint.Clicked += (_, _) => SelectPayment("TWINT");
        pendingPayment.Clicked += async (_, _) =>
        {
            try { ShowOverview(0, 0, Array.Empty<RestaurantCartLine>(), pendingOnly: true); }
            catch (Exception ex) { await DisplayAlertAsync("Zahlungsstatus", ex.Message, "OK"); }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (keepOverviewOnAppearing) return;
        try
        {
            loading = true;
            persons.Text = Preferences.Default.Get("restaurant_persons", "1");
            table.Text = Preferences.Default.Get("restaurant_table", "");
            payment = Preferences.Default.Get("restaurant_payment", "TWINT");
            if (string.IsNullOrEmpty(payment)) payment = "TWINT";
            loading = false;
            ValidatePersons();
            ShowDetails();
        }
        catch (Exception ex)
        {
            loading = false;
            await DisplayAlertAsync("Warenkorb", $"Die Daten konnten nicht geladen werden: {ex.Message}", "OK");
        }
    }

    private static Label ErrorLabel()
    {
        var label = new Label { FontSize = 13, IsVisible = false };
        label.SetAppThemeColor(Label.TextColorProperty, ErrorColor, Color.FromArgb("#FCA5A5"));
        return label;
    }

    private static bool KeepDigitsOnly(Entry entry, string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        var digits = new string(text.Where(c => c is >= '0' and <= '9').ToArray());
        if (digits == text) return false;
        var cursorPosition = Math.Clamp(entry.CursorPosition, 0, text.Length);
        var digitsBeforeCursor = text.Take(cursorPosition).Count(c => c is >= '0' and <= '9');
        entry.Text = digits;
        entry.CursorPosition = Math.Min(digitsBeforeCursor, digits.Length);
        return true;
    }

#if ANDROID
    private static void RemoveAndroidUnderline(object? sender, EventArgs e)
    {
        if (sender is Entry { Handler.PlatformView: Android.Widget.EditText nativeEntry })
            nativeEntry.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
    }
#endif

    private static Border InputBorder(View content)
    {
        var border = new Border
        {
            Stroke = FieldStroke,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = new Thickness(6, 0),
            Content = content
        };
        RestaurantUi.SurfaceTheme(border);
        return border;
    }

    private void ChangePersonCount(int change)
    {
        if (!int.TryParse(persons.Text, out var count) || count < 1 || count > 8)
        {
            ValidatePersons();
            return;
        }
        if (change > 0 && count == 8)
        {
            SetError(personsError, "Maximum 8 Personen pro Tisch.", true);
            personsBorder.Stroke = ErrorColor;
            return;
        }
        if (count + change is >= 1 and <= 8)
            persons.Text = (count + change).ToString();
    }

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
        paymentError.IsVisible = false;
        UpdatePaymentButtons();
        SaveInputs();
    }

    private void UpdatePaymentButtons()
    {
        StylePaymentButton(twint, payment == "TWINT");
        StylePaymentButton(card, payment == "Kreditkarte");
    }

    private void StylePaymentButton(Button button, bool selected)
    {
        button.BackgroundColor = selected ? RestaurantUi.Blue : Colors.White;
        button.TextColor = selected ? Colors.White : RestaurantUi.Ink;
        button.BorderColor = paymentError.IsVisible ? ErrorColor : selected ? RestaurantUi.Blue : FieldStroke;
        button.BorderWidth = 1;
        button.CornerRadius = 8;
        button.FontAttributes = FontAttributes.Bold;
        button.FontSize = 14;
        button.MinimumHeightRequest = 48;
        if (!selected)
        {
            RestaurantUi.SurfaceTheme(button);
            button.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        }
    }

    private void ShowDetails()
    {
        keepOverviewOnAppearing = false;
        Title = "Warenkorb";
        pendingPayment.IsVisible = !string.IsNullOrWhiteSpace(Preferences.Default.Get("restaurant_stripe_session", string.Empty));
        var cart = RestaurantOrderService.GetCart();
        menuCount.Text = $"{cart.Sum(line => line.Quantity)} Menüs";
        total.Text = $"CHF {cart.Sum(line => line.LineTotal):F2}";
        if (cartError.IsVisible)
        {
            SetError(cartError, "Bitte zuerst ein Menü auswählen.", cart.Count == 0);
            summaryBorder.Stroke = cartError.IsVisible ? ErrorColor : FieldStroke;
        }
        UpdatePaymentButtons();
        if (detailsView is not null)
        {
            Content = detailsView;
            return;
        }

        var minus = new Button { Text = "−", BackgroundColor = Colors.White, TextColor = RestaurantUi.Ink, BorderColor = FieldStroke, BorderWidth = 1, CornerRadius = 8, WidthRequest = 44, MinimumHeightRequest = 48, Padding = 0 };
        var plus = new Button { Text = "+", BackgroundColor = Colors.White, TextColor = RestaurantUi.Ink, BorderColor = FieldStroke, BorderWidth = 1, CornerRadius = 8, WidthRequest = 44, MinimumHeightRequest = 48, Padding = 0 };
        minus.Clicked += (_, _) => ChangePersonCount(-1);
        plus.Clicked += (_, _) => ChangePersonCount(1);
        RestaurantUi.SurfaceTheme(minus);
        RestaurantUi.SurfaceTheme(plus);
        minus.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        plus.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        var personControls = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
            ColumnSpacing = 4
        };
        personControls.Add(minus);
        personControls.Add(personsBorder, 1);
        personControls.Add(plus, 2);
        var fields = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 12
        };
        fields.Add(new VerticalStackLayout { Spacing = 6, Children = { RestaurantUi.Heading("Anzahl Personen", 15), personControls } });
        fields.Add(new VerticalStackLayout { Spacing = 6, Children = { RestaurantUi.Heading("Tischnummer", 15), tableBorder, tableError, RestaurantUi.Caption("Steht auf deinem Tisch; die Verfügbarkeit wird nicht geprüft.") } }, 1);
        var progress = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 4
        };
        progress.Add(new BoxView { Color = Color.FromArgb("#0D9488"), HeightRequest = 4 });
        progress.Add(new BoxView { Color = Color.FromArgb("#E2E8F0"), HeightRequest = 4 }, 1);
        var paymentButtons = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 10 };
        paymentButtons.Add(twint, 0, 0);
        paymentButtons.Add(card, 1, 0);
        var next = RestaurantUi.PrimaryButton("Weiter zur Übersicht");
        next.Clicked += OnContinue;
        var menu = new Button { Text = "Menü ändern", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
        menu.Clicked += async (_, _) => await OpenMenuSelectionAsync();
        detailsView = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 14,
            Children =
            {
                progress, RestaurantUi.Caption("Schritt 1 von 2: Angaben"), fields,
                RestaurantUi.Caption("Maximum 8 Personen pro Tisch."), personsError,
                RestaurantUi.Heading("Zahlungsart", 17), paymentButtons, paymentError,
                RestaurantUi.Caption("Bargeld ist nicht verfügbar. Karte oder TWINT im Stripe-Checkout, sofern verfügbar; nur Testzahlungen."),
                summaryBorder, cartError, pendingPayment, next, menu
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

    private bool ValidatePersons()
    {
        var validNumber = int.TryParse(persons.Text, out var count);
        var invalid = !validNumber || count < 1 || count > 8;
        var message = validNumber && count > 8
            ? "Maximum 8 Personen pro Tisch."
            : "Bitte mindestens 1 Person eingeben.";
        SetError(personsError, message, invalid);
        personsBorder.Stroke = invalid ? ErrorColor : FieldStroke;
        return invalid;
    }

    private bool ValidateTable()
    {
        var invalid = SetError(tableError, "Bitte eine gültige Tischnummer eingeben.", !int.TryParse(table.Text, out var number) || number < 1);
        tableBorder.Stroke = invalid ? ErrorColor : FieldStroke;
        return invalid;
    }

    private async void OnContinue(object? sender, EventArgs e)
    {
        try
        {
            var invalidPersons = ValidatePersons();
            var invalidTable = ValidateTable();
            var invalidPayment = SetError(paymentError, "Bitte Karte oder TWINT auswählen.", payment is not ("Kreditkarte" or "TWINT"));
            UpdatePaymentButtons();
            var cart = RestaurantOrderService.GetCart();
            var invalidCart = SetError(cartError, "Bitte zuerst ein Menü auswählen.", cart.Count == 0);
            summaryBorder.Stroke = invalidCart ? ErrorColor : FieldStroke;
            if (invalidPersons || invalidTable || invalidPayment || invalidCart) return;
            ShowOverview(int.Parse(persons.Text!), int.Parse(table.Text!), cart);
        }
        catch (Exception ex) { await DisplayAlertAsync("Warenkorb", $"Die Übersicht konnte nicht geladen werden: {ex.Message}", "OK"); }
    }

    private void ShowOverview(int personCount, int tableNumber, IReadOnlyList<RestaurantCartLine> cart, bool pendingOnly = false)
    {
        Title = pendingOnly ? "Zahlungsstatus" : "Übersicht";
        var details = new VerticalStackLayout { Spacing = 0 };
        if (pendingOnly)
            details.Children.Add(RestaurantUi.Caption("Der Status der offenen Stripe-Testzahlung wird geprüft."));
        else
        {
            AddOverviewRow("Tisch", tableNumber.ToString());
            AddOverviewRow("Personen", personCount.ToString());
            foreach (var line in cart) AddOverviewRow($"{line.Item.Name} × {line.Quantity}", $"CHF {line.LineTotal:F2}");
            AddOverviewRow("Zahlungswunsch", payment);
            AddOverviewRow("Total", $"CHF {cart.Sum(line => line.LineTotal):F2}", last: true);
        }

        void AddOverviewRow(string label, string value, bool last = false)
        {
            var row = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                ColumnSpacing = 12,
                Padding = new Thickness(0, 10)
            };
            var nameLabel = new Label { Text = label, FontSize = 15 };
            var valueLabel = new Label { Text = value, FontSize = 15, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.End };
            RestaurantUi.TextTheme(nameLabel);
            RestaurantUi.TextTheme(valueLabel);
            row.Add(nameLabel);
            row.Add(valueLabel, 1);
            details.Children.Add(row);
            if (!last) details.Children.Add(new BoxView { Color = Color.FromArgb("#E2E8F0"), HeightRequest = 1 });
        }

        var done = RestaurantUi.PrimaryButton("Fertig");
        done.Clicked += async (_, _) =>
        {
            try { ShowDetails(); }
            catch (Exception ex) { await DisplayAlertAsync("Warenkorb", $"Die Daten konnten nicht geladen werden: {ex.Message}", "OK"); }
        };
        var confirmation = new Grid
        {
            BackgroundColor = Color.FromArgb("#990F172A"),
            IsVisible = false,
            Padding = 24
        };
        var checkmark = new Border
        {
            BackgroundColor = Color.FromArgb("#15803D"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 36 },
            WidthRequest = 64,
            HeightRequest = 64,
            HorizontalOptions = LayoutOptions.Center,
            Content = new Label { Text = FluentUI.checkmark_24_regular, FontFamily = FluentUI.FontFamily, FontSize = 32, TextColor = Colors.White, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
        };
        var message = RestaurantUi.Caption(string.Empty);
        message.HorizontalTextAlignment = TextAlignment.Center;
        var heading = RestaurantUi.Heading("Testzahlung erfolgreich!", 20);
        heading.HorizontalTextAlignment = TextAlignment.Center;
        var confirmationCard = RestaurantUi.Card(new VerticalStackLayout { Spacing = 16, Children = { checkmark, heading, message, done } });
        confirmationCard.MaximumWidthRequest = 380;
        confirmationCard.HorizontalOptions = LayoutOptions.Fill;
        confirmationCard.VerticalOptions = LayoutOptions.Center;
        confirmation.Add(confirmationCard);

        var stripePay = RestaurantUi.PrimaryButton("Jetzt mit Stripe bezahlen");
        var checkPayment = new Button { Text = "Zahlung prüfen", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue };
        stripePay.Clicked += async (_, _) =>
        {
            stripePay.IsEnabled = false;
            checkPayment.IsEnabled = false;
            keepOverviewOnAppearing = true;
            try
            {
                if (!string.IsNullOrWhiteSpace(Preferences.Default.Get("restaurant_stripe_session", string.Empty)))
                {
                    await DisplayAlertAsync("Offene Zahlung", "Es gibt bereits eine offene Testzahlung. Bitte zuerst „Zahlung prüfen“ auswählen.", "OK");
                    return;
                }
                var checkout = await RestaurantStripeClient.CreateCheckoutAsync(cart, personCount, tableNumber);
                Preferences.Default.Set("restaurant_stripe_cart", SerializeCart(cart));
                Preferences.Default.Set("restaurant_stripe_session", checkout.SessionId);
#if ANDROID
                try
                {
                    var result = await WebAuthenticator.Default.AuthenticateAsync(checkout.CheckoutUrl, new Uri("rogerrestaurant://stripe-return"));
                    if (result.Properties.TryGetValue("result", out var returnResult) && returnResult == "success")
                        await CheckPaymentAsync();
                    else
                        await DisplayAlertAsync("Stripe-Testzahlung", "Der Checkout wurde nicht abgeschlossen. Du kannst den Status mit „Zahlung prüfen“ kontrollieren.", "OK");
                }
                catch (TaskCanceledException)
                {
                    await DisplayAlertAsync("Stripe-Testzahlung", "Der Browser wurde geschlossen. Prüfe den Zahlungsstatus, bevor du erneut bezahlst.", "OK");
                }
#else
                await Browser.Default.OpenAsync(checkout.CheckoutUrl, BrowserLaunchMode.SystemPreferred);
#endif
            }
            catch (Exception ex) { await DisplayAlertAsync("Stripe-Testzahlung", ex.Message, "OK"); }
            finally
            {
                stripePay.IsEnabled = true;
                checkPayment.IsEnabled = true;
            }
        };
        checkPayment.Clicked += async (_, _) => await CheckPaymentAsync();

        async Task CheckPaymentAsync()
        {
            var paymentWasEnabled = stripePay.IsEnabled;
            stripePay.IsEnabled = false;
            checkPayment.IsEnabled = false;
            try
            {
                var sessionId = Preferences.Default.Get("restaurant_stripe_session", string.Empty);
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    await DisplayAlertAsync("Zahlungsstatus", "Starte zuerst die Stripe-Testzahlung.", "OK");
                    return;
                }
                var details = await RestaurantStripeClient.GetPaymentDetailsAsync(sessionId);
                if (!details.Paid)
                {
                    var discard = await DisplayAlertAsync("Zahlungsstatus", "Stripe hat die Zahlung noch nicht bestätigt. Wenn du den offenen Checkout verwirfst, wird eine später bestätigte Zahlung nicht mehr automatisch erkannt. Nur verwerfen, wenn du sicher bist, dass du den Checkout nicht mehr bezahlst.", "Checkout verwerfen", "Behalten");
                    if (discard)
                    {
                        Preferences.Default.Remove("restaurant_stripe_session");
                        Preferences.Default.Remove("restaurant_stripe_cart");
                    }
                    return;
                }

                await StripePurchaseHistory.AddPaidSessionAsync(details.SessionId);
                var savedCart = Preferences.Default.Get("restaurant_stripe_cart", string.Empty);
                var cartUnchanged = !string.IsNullOrEmpty(savedCart) && savedCart == SerializeCart(RestaurantOrderService.GetCart());
                if (cartUnchanged) RestaurantOrderService.Clear();
                Preferences.Default.Remove("restaurant_stripe_session");
                Preferences.Default.Remove("restaurant_stripe_cart");
                var method = details.PaymentMethod switch
                {
                    "card" => "Karte",
                    "twint" => "TWINT",
                    _ => details.PaymentMethod
                };
                message.Text = $"Stripe hat {details.Amount:F2} {details.Currency} mit {method} bestätigt. Es wurde kein echter Betrag belastet. Die Bestellung ist im Verlauf auf diesem Gerät gespeichert.";
                if (!cartUnchanged)
                    message.Text += " Dein aktueller Warenkorb wurde nicht verändert.";
                keepOverviewOnAppearing = true;
                confirmation.IsVisible = true;
            }
            catch (Exception ex) { await DisplayAlertAsync("Zahlungsstatus", ex.Message, "OK"); }
            finally
            {
                stripePay.IsEnabled = paymentWasEnabled;
                checkPayment.IsEnabled = true;
            }
        }
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
        edit.Clicked += async (_, _) => await OpenMenuSelectionAsync();
        change.Text = pendingOnly ? "Zurück zum Warenkorb" : "Bestellung ändern";
        stripePay.IsVisible = !pendingOnly;
        save.IsVisible = !pendingOnly;
        edit.IsVisible = !pendingOnly;
        change.BorderColor = RestaurantUi.Blue;
        change.BorderWidth = 1;
        change.CornerRadius = 8;
        var progress = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 4
        };
        progress.Add(new BoxView { Color = Color.FromArgb("#0D9488"), HeightRequest = 4 });
        progress.Add(new BoxView { Color = Color.FromArgb("#0D9488"), HeightRequest = 4 }, 1);
        var overview = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                progress, RestaurantUi.Caption(pendingOnly ? "Offene Testzahlung" : "Schritt 2 von 2: Übersicht"), RestaurantUi.Heading(pendingOnly ? "Zahlung prüfen" : "Stimmt alles?"),
                RestaurantUi.Caption(pendingOnly ? "Stripe bestätigt den tatsächlichen Status." : "Bitte prüfe deine Auswahl."), RestaurantUi.Card(details),
                stripePay, change,
                RestaurantUi.Caption("Testzahlung: Die tatsächliche Zahlungsart wird im Stripe-Checkout gewählt, sofern verfügbar. Es wird kein echtes Geld belastet."),
                checkPayment, save, edit
            }
        });
        Content = new Grid { Children = { overview, confirmation } };
        if (pendingOnly) Dispatcher.Dispatch(async () => await CheckPaymentAsync());
    }

    private static string SerializeCart(IReadOnlyList<RestaurantCartLine> cart) =>
        JsonSerializer.Serialize(cart.OrderBy(line => line.Item.Id, StringComparer.Ordinal)
            .Select(line => new { line.Item.Id, line.Quantity }));

    private static async Task OpenMenuSelectionAsync()
    {
        await Shell.Current.GoToAsync("//menu");
        await Shell.Current.GoToAsync("menu-selection");
    }
}
