using NET_MAUI_App.Models;
using NET_MAUI_App.PageModels;

namespace NET_MAUI_App.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}