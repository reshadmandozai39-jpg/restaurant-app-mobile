using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class HomePage : ContentPage
    {
        private readonly Label greeting = RestaurantUi.Heading("Willkommen!");

        public HomePage()
        {
            Title = "Start";
            BackgroundColor = RestaurantUi.Background;
            var menuButton = RestaurantUi.PrimaryButton("Menü ansehen");
            menuButton.Command = new Command(async () => await Shell.Current.GoToAsync("//menu"));
            var cartButton = RestaurantUi.PrimaryButton("Zum Warenkorb");
            cartButton.Command = new Command(async () => await Shell.Current.GoToAsync("//cart"));
            var offers = new VerticalStackLayout { Spacing = 10 };
            foreach (var item in RestaurantOrderService.Menu)
            {
                offers.Children.Add(RestaurantUi.Card(new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        RestaurantUi.Heading($"{item.Emoji}  {item.Name}", 16),
                        RestaurantUi.Caption(item.Description),
                        RestaurantUi.Caption($"CHF {item.Price:F2}")
                    }
                }));
            }

            Content = RestaurantUi.PageContent(new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    greeting,
                    RestaurantUi.Card(new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children = { RestaurantUi.Heading("Hunger?", 18), RestaurantUi.Caption("Entdecke unsere Menüs und bestelle direkt über dein Smartphone."), menuButton }
                    }),
                    RestaurantUi.Heading("Heute im Angebot", 18),
                    offers,
                    cartButton,
                    RestaurantUi.Caption("Frisch gekocht in Bern · Preise in CHF")
                }
            });
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                greeting.Text = $"Grüezi, {await new LocalAccountService().GetDisplayNameAsync()}";
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Konto", $"Der Name konnte nicht geladen werden: {ex.Message}", "OK");
            }
        }
    }
}
