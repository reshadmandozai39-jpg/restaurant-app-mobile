using Microsoft.Extensions.DependencyInjection;

namespace NET_MAUI_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new Pages.LoginPage()));
        }
    }
}