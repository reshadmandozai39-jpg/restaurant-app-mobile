using NET_MAUI_App.Services;

namespace NET_MAUI_App.Pages
{
    public class MenuPage : ContentPage
    {
        private readonly VerticalStackLayout menuList = new() { Spacing = 10 };
        private readonly Label cartSummary = RestaurantUi.Caption(string.Empty);
        private readonly HorizontalStackLayout filters = new() { Spacing = 8 };
        private RestaurantMenuCategory? selectedCategory;

        public MenuPage()
        {
            Title = "Menü auswählen";
            BackgroundColor = RestaurantUi.Background;
            var cartButton = RestaurantUi.PrimaryButton("Zum Warenkorb");
            cartButton.Command = new Command(async () => await Shell.Current.GoToAsync("//cart"));
            Content = RestaurantUi.PageContent(new VerticalStackLayout
            {
                Spacing = 14,
                Children =
                {
                    RestaurantUi.Heading("Heute im Angebot"),
                    RestaurantUi.Caption("Beispielmenüs und Beispielpreise · Bargeld und TWINT nicht verfügbar · Kreditkarte nur im Stripe-Testmodus."),
                    new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = filters },
                    menuList, cartSummary, cartButton
                }
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshMenu();
        }

        private async void RefreshMenu()
        {
            try { BuildMenu(); }
            catch (Exception ex) { await DisplayAlertAsync("Menü", $"Die Daten konnten nicht geladen werden: {ex.Message}", "OK"); }
        }

        private async void ChangeQuantity(string itemId, bool add)
        {
            try
            {
                if (add) RestaurantOrderService.Add(itemId);
                else RestaurantOrderService.RemoveOne(itemId);
                BuildMenu();
            }
            catch (Exception ex) { await DisplayAlertAsync("Warenkorb", $"Die Änderung konnte nicht gespeichert werden: {ex.Message}", "OK"); }
        }

        private void BuildMenu()
        {
            filters.Children.Clear();
            AddFilter("Alle", null);
            AddFilter("Tagesmenü", RestaurantMenuCategory.Tagesmenue);
            AddFilter("Vegi", RestaurantMenuCategory.Vegi);
            AddFilter("Kinder", RestaurantMenuCategory.Kinder);

            menuList.Children.Clear();
            var cart = RestaurantOrderService.GetCart();
            var counts = RestaurantStorage.GetDraftCounts();
            var items = RestaurantOrderService.Menu.Where(item => selectedCategory is null || item.Category == selectedCategory).ToArray();
            if (items.Length == 0)
                menuList.Children.Add(RestaurantUi.Caption("Für diese Kategorie sind noch keine Menüs hinterlegt."));

            foreach (var item in items)
            {
                var quantity = cart.FirstOrDefault(line => line.Item.Id == item.Id)?.Quantity ?? 0;
                var minus = new Button { Text = "−", FontSize = 16, BackgroundColor = Color.FromArgb("#EFF3F9"), TextColor = RestaurantUi.Ink, IsEnabled = quantity > 0 };
                minus.Clicked += (_, _) => ChangeQuantity(item.Id, false);
                var plus = RestaurantUi.PrimaryButton("+");
                plus.Clicked += (_, _) => ChangeQuantity(item.Id, true);
                menuList.Children.Add(RestaurantUi.Card(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        RestaurantUi.Heading($"{item.Emoji}  {item.Name}", 17),
                        RestaurantUi.Caption(item.Description),
                        RestaurantUi.Heading($"CHF {item.Price:F2}", 15),
                        RestaurantUi.Caption($"{counts.GetValueOrDefault(item.Id)}× in gespeicherten Entwürfen"),
                        new HorizontalStackLayout
                        {
                            Spacing = 12,
                            Children = { minus, new Label { Text = quantity.ToString(), TextColor = RestaurantUi.Ink, VerticalOptions = LayoutOptions.Center }, plus }
                        }
                    }
                }));
            }
            cartSummary.Text = $"{cart.Sum(line => line.Quantity)} Menüs · CHF {RestaurantOrderService.GetTotal():F2}";
        }

        private void AddFilter(string title, RestaurantMenuCategory? category)
        {
            var button = new Button
            {
                Text = title, FontSize = 13, CornerRadius = 20,
                BackgroundColor = selectedCategory == category ? RestaurantUi.Blue : Colors.White,
                TextColor = selectedCategory == category ? Colors.White : RestaurantUi.Ink
            };
            button.Clicked += (_, _) => { selectedCategory = category; RefreshMenu(); };
            filters.Children.Add(button);
        }
    }
}
