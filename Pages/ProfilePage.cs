using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class ProfilePage : ContentPage
    {
        private readonly Label nameLabel = RestaurantUi.Heading("Mein Konto", 18);
        private readonly Label draftCount = RestaurantUi.Caption(string.Empty);
        private readonly Entry backendUrl = new()
        {
            Placeholder = "https://dein-backend.example.com",
            Keyboard = Keyboard.Url,
            TextColor = RestaurantUi.Ink,
            PlaceholderColor = RestaurantUi.Muted,
            FontSize = 14,
            BackgroundColor = Colors.White
        };
        private readonly Label backendStatus = RestaurantUi.Caption("Für Android ist eine öffentliche HTTPS-Adresse erforderlich.");

        public ProfilePage()
        {
            Title = "Mein Konto";
            BackgroundColor = RestaurantUi.Background;

            var logoutButton = RestaurantUi.PrimaryButton("Abmelden");
            logoutButton.Clicked += OnLogoutClicked;

            Content = RestaurantUi.PageContent(new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    RestaurantUi.Heading("Mein Konto"),
                    RestaurantUi.Card(new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children = { nameLabel, RestaurantUi.Caption("Mitglied · Roger Restaurant Bern") }
                    }),
                    RestaurantUi.Card(new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            RestaurantUi.Heading("Gratis-Menü", 17),
                            RestaurantUi.Caption("Das Bonusprogramm ist noch nicht angebunden.")
                        }
                    }),
                    RestaurantUi.Card(new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            RestaurantUi.Heading("Bestellungen", 17),
                            draftCount,
                            RestaurantUi.Caption("Entwürfe sind nicht bezahlt und werden nicht an das Restaurant gesendet."),
                            new Button
                            {
                                Text = "Gespeicherte Entwürfe anzeigen", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue,
                                Command = new Command(async () => await Navigation.PushAsync(new OrderHistoryPage()))
                            }
                        }
                    }),
                    RestaurantUi.Card(new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            RestaurantUi.Heading("Backend für Android", 17),
                            RestaurantUi.Caption("Trage hier die HTTPS-Adresse deines gehosteten Backends ein."),
                            backendUrl,
                            backendStatus,
                            new Button
                            {
                                Text = "Backend-Adresse speichern", BackgroundColor = Colors.White, TextColor = RestaurantUi.Blue,
                                Command = new Command(async () => await SaveBackendUrlAsync())
                            }
                        }
                    }),
                    logoutButton
                }
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                nameLabel.Text = await new LocalAccountService().GetDisplayNameAsync();
                draftCount.Text = $"{RestaurantStorage.GetDrafts().Count} gespeicherte Entwürfe";
                backendUrl.Text = Preferences.Default.Get("restaurant_backend_url", string.Empty);
                backendStatus.Text = string.IsNullOrWhiteSpace(backendUrl.Text)
                    ? "Noch keine Backend-Adresse gespeichert."
                    : "Backend-Adresse ist auf diesem Gerät gespeichert.";
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Konto", $"Der Name konnte nicht geladen werden: {ex.Message}", "OK");
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

        private void OnLogoutClicked(object? sender, EventArgs e)
        {
            if (Window is not null)
                Window.Page = new NavigationPage(new LoginPage());
        }
    }
}
