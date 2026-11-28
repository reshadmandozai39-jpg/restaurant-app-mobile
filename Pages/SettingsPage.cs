namespace NET_MAUI_App.Pages;

public class SettingsPage : ContentPage
{
    private readonly Entry backendUrl = new()
    {
        Placeholder = "https://dein-backend.example.com",
        Keyboard = Keyboard.Url,
        TextColor = RestaurantUi.Ink,
        PlaceholderColor = RestaurantUi.Muted,
        FontSize = 14,
        BackgroundColor = Colors.White
    };
    private readonly Label backendStatus = RestaurantUi.Caption(string.Empty);

    public SettingsPage()
    {
        Title = "Einstellungen";
        RestaurantUi.PageTheme(this);
        RestaurantUi.SurfaceTheme(backendUrl);
        backendUrl.SetAppThemeColor(Entry.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        var appearance = new Picker { Title = "Darstellung auswählen", ItemsSource = new[] { "Hell", "Dunkel" } };
        appearance.SetAppThemeColor(Picker.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
        appearance.SelectedIndex = Application.Current?.UserAppTheme == AppTheme.Dark ? 1 : 0;
        appearance.SelectedIndexChanged += async (_, _) =>
        {
            if (appearance.SelectedIndex < 0) return;
            try
            {
                var app = Application.Current ?? throw new InvalidOperationException("Die Anwendung ist nicht verfügbar.");
                var dark = appearance.SelectedIndex == 1;
                Preferences.Default.Set("restaurant_theme", dark ? "dark" : "light");
                app.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Darstellung", $"Die Einstellung konnte nicht gespeichert werden: {ex.Message}", "OK");
            }
        };
        var save = RestaurantUi.PrimaryButton("Backend-Adresse speichern");
        save.Clicked += async (_, _) => await SaveBackendUrlAsync();
        Content = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                RestaurantUi.Card(new VerticalStackLayout { Spacing = 8, Children = { RestaurantUi.Heading("Darstellung", 17), appearance } }),
                RestaurantUi.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        RestaurantUi.Heading("Backend für Android", 17),
                        RestaurantUi.Caption("Trage hier die HTTPS-Adresse deines gehosteten Backends ein."),
                        backendUrl, backendStatus, save
                    }
                })
            }
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            backendUrl.Text = Preferences.Default.Get("restaurant_backend_url", string.Empty);
            backendStatus.Text = string.IsNullOrWhiteSpace(backendUrl.Text)
                ? "Noch keine Backend-Adresse gespeichert."
                : "Backend-Adresse ist auf diesem Gerät gespeichert.";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Einstellungen", $"Die Backend-Adresse konnte nicht geladen werden: {ex.Message}", "OK");
        }
    }

    private async Task SaveBackendUrlAsync()
    {
        var value = backendUrl.Text?.Trim() ?? string.Empty;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            string.IsNullOrWhiteSpace(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
        {
            await DisplayAlertAsync("Backend-Adresse", "Bitte eine gültige öffentliche HTTPS-Adresse ohne Benutzername, Passwort oder Abfrageparameter eingeben.", "OK");
            return;
        }

        try
        {
            Preferences.Default.Set("restaurant_backend_url", uri.AbsoluteUri.TrimEnd('/'));
            backendStatus.Text = "Backend-Adresse ist auf diesem Gerät gespeichert.";
            await DisplayAlertAsync("Backend-Adresse", "Gespeichert. Stripe-Anfragen verwenden jetzt diese HTTPS-Adresse.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Speichern fehlgeschlagen", ex.Message, "OK");
        }
    }
}
