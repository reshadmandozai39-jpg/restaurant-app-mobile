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
            LoginError.IsVisible = false;

            try
            {
                var email = EmailEntry.Text?.Trim() ?? string.Empty;
                var password = PasswordEntry.Text ?? string.Empty;
                EmailError.IsVisible = string.IsNullOrWhiteSpace(email);
                PasswordError.IsVisible = string.IsNullOrEmpty(password);
                if (EmailError.IsVisible || PasswordError.IsVisible)
                    return;

                if (!await accountService.AccountExistsAsync())
                {
                    LoginError.Text = "Bitte erstelle zuerst ein Konto auf diesem Gerät.";
                    LoginError.IsVisible = true;
                    return;
                }

                if (!await accountService.VerifyAsync(email, password))
                {
                    LoginError.Text = "E-Mail oder Passwort ist falsch.";
                    LoginError.IsVisible = true;
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
