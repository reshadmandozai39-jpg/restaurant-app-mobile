using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LocalAccountService accountService = new();

        public LoginPage()
        {
            InitializeComponent();
        }

        private async void OnRegisterClicked(object? sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

        private async void OnLoginClicked(object? sender, EventArgs e)
        {
            LoginButton.IsEnabled = false;
            CreateAccountButton.IsEnabled = false;
            try
            {
                var email = EmailEntry.Text?.Trim() ?? string.Empty;
                var password = PasswordEntry.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                {
                    await DisplayAlertAsync("Anmelden", "Bitte gib E-Mail und Passwort ein.", "OK");
                    return;
                }

                if (!await accountService.VerifyAsync(email, password))
                {
                    await DisplayAlertAsync("Anmelden", "E-Mail oder Passwort ist falsch.", "OK");
                    return;
                }

                PasswordEntry.Text = string.Empty;
                if (Window is null)
                    throw new InvalidOperationException("Das App-Fenster ist nicht verfügbar.");

                Window.Page = new AppShell();
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Anmelden fehlgeschlagen", ex.Message, "OK");
            }
            finally
            {
                LoginButton.IsEnabled = true;
                CreateAccountButton.IsEnabled = true;
            }
        }
    }
}
