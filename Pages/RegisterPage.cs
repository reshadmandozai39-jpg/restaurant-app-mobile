using System.Net.Mail;
using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class RegisterPage : ContentPage
    {
        private readonly Entry nameEntry = new() { Placeholder = "Dein Name", TextColor = Color.FromArgb("#1F2933") };
        private readonly Entry emailEntry = new() { Placeholder = "Deine E-Mail-Adresse", Keyboard = Keyboard.Email, TextColor = Color.FromArgb("#1F2933") };
        private readonly Entry passwordEntry = new() { Placeholder = "Passwort (mindestens 8 Zeichen)", IsPassword = true, TextColor = Color.FromArgb("#1F2933") };
        private readonly Entry confirmPasswordEntry = new() { Placeholder = "Passwort wiederholen", IsPassword = true, TextColor = Color.FromArgb("#1F2933") };
        private readonly Button createButton = new() { Text = "Konto erstellen" };

        public RegisterPage()
        {
            Title = "Registrieren";
            BackgroundColor = RestaurantUi.Background;
            createButton.BackgroundColor = RestaurantUi.Blue;
            createButton.TextColor = Colors.White;
            createButton.Clicked += OnCreateAccountClicked;

            foreach (var entry in new[] { nameEntry, emailEntry, passwordEntry, confirmPasswordEntry })
            {
                entry.BackgroundColor = Colors.White;
                entry.PlaceholderColor = RestaurantUi.Muted;
                entry.FontSize = 16;
            }

            Content = RestaurantUi.PageContent(new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    new Image { Source = "logo.png", HeightRequest = 100, WidthRequest = 100, Aspect = Aspect.AspectFit },
                    RestaurantUi.Heading("Konto erstellen"),
                    RestaurantUi.Caption("Das Konto wird nur auf diesem Gerät gespeichert."),
                    nameEntry, emailEntry, passwordEntry, confirmPasswordEntry, createButton,
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

                if (name.Length is < 1 or > 80)
                {
                    await DisplayAlertAsync("Name", "Bitte gib einen Namen mit höchstens 80 Zeichen ein.", "OK");
                    return;
                }
                if (!MailAddress.TryCreate(email, out var address) ||
                    !string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
                {
                    await DisplayAlertAsync("E-Mail", "Bitte gib eine gültige E-Mail-Adresse ein.", "OK");
                    return;
                }
                if (password.Length < 8)
                {
                    await DisplayAlertAsync("Passwort", "Das Passwort muss mindestens 8 Zeichen lang sein.", "OK");
                    return;
                }
                if (password != confirmPasswordEntry.Text)
                {
                    await DisplayAlertAsync("Passwort", "Die Passwörter stimmen nicht überein.", "OK");
                    return;
                }

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
