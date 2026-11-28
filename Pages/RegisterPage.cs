using System.Net.Mail;
using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class RegisterPage : ContentPage
    {
        private readonly Entry nameEntry = new() { Placeholder = "Dein Name",  };
        private readonly Entry emailEntry = new() { Placeholder = "Deine E-Mail-Adresse", Keyboard = Keyboard.Email };
        private readonly Entry passwordEntry = new() { Placeholder = "Passwort (mindestens 8 Zeichen)", IsPassword = true };
        private readonly Entry confirmPasswordEntry = new() { Placeholder = "Passwort wiederholen", IsPassword = true  };
        private readonly Button createButton = new() { Text = "Konto erstellen" };
        private readonly Label nameError = new() { TextColor = Colors.Red, IsVisible = false };
        private readonly Label emailError = new() { TextColor = Colors.Red, IsVisible = false };
        private readonly Label passwordError = new() { TextColor = Colors.Red, IsVisible = false };
        private readonly Label confirmPasswordError = new() { TextColor = Colors.Red, IsVisible = false };

        public RegisterPage()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            RestaurantUi.PageTheme(this);
            createButton.BackgroundColor = RestaurantUi.Blue;
            createButton.TextColor = Colors.White;
            createButton.Clicked += OnCreateAccountClicked;

            foreach (var entry in new[] { nameEntry, emailEntry, passwordEntry, confirmPasswordEntry })
            {
                RestaurantUi.SurfaceTheme(entry);
                entry.SetAppThemeColor(Entry.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
                entry.SetAppThemeColor(Entry.PlaceholderColorProperty, RestaurantUi.Muted, RestaurantUi.DarkMuted);
                entry.FontSize = 16;
            }

            Content = RestaurantUi.PageContent(new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    new Image { Source = "logo.png", HeightRequest = 100, WidthRequest = 100, Aspect = Aspect.AspectFit },
                    RestaurantUi.Heading("Konto erstellen"),
                    nameEntry, nameError,
                    emailEntry, emailError,
                    passwordEntry, passwordError,
                    confirmPasswordEntry, confirmPasswordError,
                    createButton,
                    new Button
                    {
                        Text = "Zur Anmeldung", BackgroundColor = Colors.Transparent, TextColor = RestaurantUi.Blue,
                        Command = new Command(async () => await Navigation.PopAsync())
                    }
                }
            });
        }

        private async void OnCreateAccountClicked(object? sender, EventArgs e)
        {
            createButton.IsEnabled = false;
            try
            {
                var name = nameEntry.Text?.Trim() ?? string.Empty;
                var email = emailEntry.Text?.Trim() ?? string.Empty;
                var password = passwordEntry.Text ?? string.Empty;
                var confirmation = confirmPasswordEntry.Text ?? string.Empty;

                nameError.Text = "Bitte gib einen Namen mit höchstens 80 Zeichen ein.";
                nameError.IsVisible = name.Length is < 1 or > 80;
                emailError.Text = "Bitte gib eine gültige E-Mail-Adresse ein.";
                emailError.IsVisible = !MailAddress.TryCreate(email, out var address) ||
                    !string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
                passwordError.Text = "Das Passwort muss mindestens 8 Zeichen lang sein.";
                passwordError.IsVisible = password.Length < 8;
                confirmPasswordError.Text = string.IsNullOrEmpty(confirmation)
                    ? "Bitte wiederhole dein Passwort."
                    : "Die Passwörter stimmen nicht überein.";
                confirmPasswordError.IsVisible = string.IsNullOrEmpty(confirmation) || password != confirmation;

                if (nameError.IsVisible || emailError.IsVisible || passwordError.IsVisible || confirmPasswordError.IsVisible)
                    return;

                await new LocalAccountService().CreateAccountAsync(name, email, password);
                await DisplayAlertAsync("Konto erstellt", "Melde dich jetzt mit deiner E-Mail und deinem Passwort an.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Konto konnte nicht erstellt werden", ex.Message, "OK");
            }
            finally
            {
                createButton.IsEnabled = true;
            }
        }
    }
}
