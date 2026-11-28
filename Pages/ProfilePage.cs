using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class ProfilePage : ContentPage
    {
        private readonly Label nameLabel = RestaurantUi.Heading("Mein Konto", 18);
        private readonly Label avatar = new() { Text = "G", TextColor = Colors.White, FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
        private readonly Label draftCount = RestaurantUi.Caption(string.Empty);

        public ProfilePage()
        {
            Title = "Mein Konto";
            RestaurantUi.PageTheme(this);

            var logoutButton = AccountButton("Abmelden", FluentUI.sign_out_24_regular);
            logoutButton.Clicked += OnLogoutClicked;
            var avatarCircle = new Border
            {
                BackgroundColor = RestaurantUi.Blue,
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 36 },
                WidthRequest = 64,
                HeightRequest = 64,
                Content = avatar
            };
            var profile = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 14
            };
            profile.Add(avatarCircle);
            profile.Add(new VerticalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center, Children = { nameLabel, RestaurantUi.Caption("Lokales Konto · Roger Restaurant Bern") } }, 1);
            var bonus = RestaurantUi.Card(new VerticalStackLayout
            {
                Spacing = 8,
                Children = { RestaurantUi.Heading("Gratis-Menü", 17), RestaurantUi.Caption("Das Bonusprogramm ist noch nicht angebunden.") }
            });
            bonus.SetAppThemeColor(VisualElement.BackgroundColorProperty, Color.FromArgb("#E8F8F3"), Color.FromArgb("#16433D"));
            var historyButton = AccountButton("Bisherige Bestellungen", FluentUI.receipt_24_regular);
            historyButton.Clicked += async (_, _) => await Navigation.PushAsync(new OrderHistoryPage());
            var paymentButton = AccountButton("Zahlungsarten", FluentUI.payment_24_regular);
            paymentButton.Clicked += async (_, _) => await DisplayAlertAsync("Zahlungsarten", "Karte und TWINT werden im Stripe-Checkout ausgewählt, sofern verfügbar. Die App speichert keine Zahlungsdaten.", "OK");
            var settingsButton = AccountButton("Einstellungen", FluentUI.settings_24_regular);
            settingsButton.Clicked += async (_, _) => await Navigation.PushAsync(new SettingsPage());
            var stats = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 10
            };
            stats.Add(RestaurantUi.Card(new VerticalStackLayout { Children = { RestaurantUi.Caption("Besuche"), RestaurantUi.Heading("0", 22) } }));
            stats.Add(RestaurantUi.Card(new VerticalStackLayout { Children = { RestaurantUi.Caption("Gesamtausgaben"), RestaurantUi.Heading("CHF 0.00", 20) } }), 1);

            Content = RestaurantUi.PageContent(new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    profile, bonus, stats,
                    RestaurantUi.Caption("Keine Online-Kontodaten angebunden; die Werte sind Platzhalter."),
                    historyButton, draftCount, paymentButton, settingsButton, logoutButton
                }
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                nameLabel.Text = await new LocalAccountService().GetDisplayNameAsync();
                var displayName = nameLabel.Text.Trim();
                avatar.Text = string.IsNullOrEmpty(displayName) ? "G" : displayName[..1].ToUpperInvariant();
                draftCount.Text = $"{RestaurantStorage.GetDrafts().Count} lokale, unbezahlte Entwürfe";
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Konto", $"Der Name konnte nicht geladen werden: {ex.Message}", "OK");
            }
        }

        private static Button AccountButton(string text, string icon)
        {
            var image = new FontImageSource { Glyph = icon, FontFamily = FluentUI.FontFamily, Size = 24 };
            image.SetAppThemeColor(FontImageSource.ColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
            var button = new Button
            {
                Text = text, ImageSource = image,
                ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 12),
                BorderColor = Color.FromArgb("#CBD5E1"), BorderWidth = 1, CornerRadius = 10,
                FontAttributes = FontAttributes.Bold, MinimumHeightRequest = 56
            };
            RestaurantUi.SurfaceTheme(button);
            button.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
            return button;
        }

        private void OnLogoutClicked(object? sender, EventArgs e)
        {
            if (Window is not null)
                Window.Page = new NavigationPage(new LoginPage());
        }
    }
}
