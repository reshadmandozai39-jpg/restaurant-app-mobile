using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class HomePage : ContentPage
    {
        private readonly Label greeting = RestaurantUi.Heading("Willkommen!");

        public HomePage()
        {
            Title = "Roger Restaurant";
            RestaurantUi.PageTheme(this);
            var title = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 10,
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(0, 4)
            };
            var logo = new Image
            {
                Source = "logo.png",
                WidthRequest = 40,
                HeightRequest = 40,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.Center
            };
            SemanticProperties.SetDescription(logo, "Logo von Roger Restaurant");
            title.Add(logo);
            title.Add(new Label
            {
                Text = "Roger Restaurant",
                TextColor = Colors.White,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            }, 1);
            Shell.SetTitleView(this, title);
            var menuButton = RestaurantUi.PrimaryButton("Menü ansehen");
            menuButton.Command = new Command(async () => await Shell.Current.GoToAsync("menu-selection"));
            var offers = new VerticalStackLayout { Spacing = 10 };
            var featured = RestaurantOrderService.Menu
                .Where(item => item.Id is "tagesmenue" or "vegi-menue");
            foreach (var item in featured)
            {
                var row = new Grid
                {
                    ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                    ColumnSpacing = 12
                };
                row.Add(new Label { Text = item.Emoji, FontSize = 26, VerticalOptions = LayoutOptions.Center });
                row.Add(new VerticalStackLayout { Spacing = 4, Children = { RestaurantUi.Heading(item.Name, 16), RestaurantUi.Caption(item.Description) } }, 1);
                row.Add(RestaurantUi.Heading($"CHF {item.Price:F2}", 15), 2);
                offers.Children.Add(RestaurantUi.Card(row));
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
                        Children = { RestaurantUi.Heading("Hunger?", 18), RestaurantUi.Caption("Entdecke unsere Beispielmenüs."), menuButton }
                    }),
                    RestaurantUi.Heading("Heute im Angebot", 18),
                    offers,
                    RestaurantUi.Card(new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children = { RestaurantUi.Heading("Gratis-Menü", 17), RestaurantUi.Caption("Das Bonusprogramm ist noch nicht angebunden.") }
                    }),
                    RestaurantUi.Caption("Beispielmenüs und Beispielpreise in CHF")
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
