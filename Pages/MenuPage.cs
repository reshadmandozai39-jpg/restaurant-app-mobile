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
            RestaurantUi.PageTheme(this);
            var cartButton = RestaurantUi.PrimaryButton("Zum Warenkorb");
            cartButton.Command = new Command(async () => await Shell.Current.GoToAsync("//cart"));
            var progress = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                ColumnSpacing = 4
            };
            progress.Add(new BoxView { Color = Color.FromArgb("#0D9488"), HeightRequest = 4 });
            progress.Add(new BoxView { Color = Color.FromArgb("#E2E8F0"), HeightRequest = 4 }, 1);
            var content = new Grid
            {
                RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto) },
                RowSpacing = 10,
                Padding = new Thickness(18, 16, 18, 12),
                MaximumWidthRequest = 600,
                HorizontalOptions = LayoutOptions.Fill
            };
            content.Add(new VerticalStackLayout
            {
                Spacing = 8,
                Children = { progress, RestaurantUi.Caption("Schritt 1 von 2: Auswahl"), new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = filters } }
            });
            content.Add(new ScrollView { Content = menuList }, 0, 1);
            content.Add(new VerticalStackLayout
            {
                Spacing = 8,
                Children = { RestaurantUi.Card(cartSummary), cartButton, RestaurantUi.Caption("Beispielmenüs und -preise · nur Stripe-Testzahlungen.") }
            }, 0, 2);
            Content = content;
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
                var minus = new Button { Text = "−", FontSize = 16, BackgroundColor = Colors.White, TextColor = RestaurantUi.Ink, BorderColor = Color.FromArgb("#CBD5E1"), BorderWidth = 1, CornerRadius = 8, WidthRequest = 40, HeightRequest = 40, Padding = 0, IsEnabled = quantity > 0 };
                minus.Clicked += (_, _) => ChangeQuantity(item.Id, false);
                var plus = new Button { Text = "+", FontSize = 16, BackgroundColor = Colors.White, TextColor = RestaurantUi.Ink, BorderColor = Color.FromArgb("#CBD5E1"), BorderWidth = 1, CornerRadius = 8, WidthRequest = 40, HeightRequest = 40, Padding = 0 };
                plus.Clicked += (_, _) => ChangeQuantity(item.Id, true);
                RestaurantUi.SurfaceTheme(minus);
                RestaurantUi.SurfaceTheme(plus);
                minus.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
                plus.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
                var heading = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, ColumnSpacing = 8 };
                heading.Add(RestaurantUi.Heading(item.Name, 16));
                heading.Add(RestaurantUi.Heading($"CHF {item.Price:F2}", 15), 1);
                var quantityLabel = RestaurantUi.Heading(quantity.ToString(), 14);
                quantityLabel.VerticalOptions = LayoutOptions.Center;
                var controls = new HorizontalStackLayout
                {
                    Spacing = 8,
                    HorizontalOptions = LayoutOptions.End,
                    Children = { minus, quantityLabel, plus }
                };
                var row = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) }, ColumnSpacing = 12 };
                row.Add(new Label { Text = item.Emoji, FontSize = 28, VerticalOptions = LayoutOptions.Center });
                row.Add(new VerticalStackLayout
                {
                    Spacing = 4,
                    Children = { heading, RestaurantUi.Caption(item.Description), RestaurantUi.Caption($"{counts.GetValueOrDefault(item.Id)}× in gespeicherten Entwürfen"), controls }
                }, 1);
                var itemCard = RestaurantUi.Card(row);
                itemCard.Padding = 12;
                if (quantity > 0)
                {
                    itemCard.Stroke = RestaurantUi.Blue;
                    itemCard.SetAppThemeColor(VisualElement.BackgroundColorProperty, Color.FromArgb("#F1F5FF"), Color.FromArgb("#1E3A5F"));
                }
                menuList.Children.Add(itemCard);
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
            if (selectedCategory != category)
            {
                RestaurantUi.SurfaceTheme(button);
                button.SetAppThemeColor(Button.TextColorProperty, RestaurantUi.Ink, RestaurantUi.DarkInk);
            }
            button.Clicked += (_, _) => { selectedCategory = category; RefreshMenu(); };
            filters.Children.Add(button);
        }
    }
}
