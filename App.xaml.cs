using Microsoft.Extensions.DependencyInjection;

namespace NET_MAUI_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            UserAppTheme = Preferences.Default.Get("restaurant_theme", "light") == "dark" ? AppTheme.Dark : AppTheme.Light;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new Pages.LoginPage()));
        }
    }
}