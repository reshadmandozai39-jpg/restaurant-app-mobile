namespace NET_MAUI_App.Services
{
    public sealed record RestaurantCartLine(RestaurantMenuItem Item, int Quantity)
    {
        public decimal LineTotal => Item.Price * Quantity;
    }

    public static class RestaurantOrderService
    {
        public static IReadOnlyList<RestaurantMenuItem> Menu => RestaurantCatalog.Menu;

        public static IReadOnlyList<RestaurantCartLine> GetCart()
        {
            return RestaurantStorage.LoadCart().Select(pair => new RestaurantCartLine(
                Menu.Single(item => item.Id == pair.Key), pair.Value)).ToArray();
        }

        public static decimal GetTotal() => GetCart().Sum(line => line.LineTotal);

        public static void Add(string itemId)
        {
            if (!Menu.Any(item => item.Id == itemId))
                throw new ArgumentException("Das Menü wurde nicht gefunden.", nameof(itemId));
            var cart = RestaurantStorage.LoadCart();
            RestaurantStorage.SetQuantity(itemId, checked(cart.GetValueOrDefault(itemId) + 1));
        }

        public static void RemoveOne(string itemId)
        {
            var cart = RestaurantStorage.LoadCart();
            if (cart.TryGetValue(itemId, out var quantity))
                RestaurantStorage.SetQuantity(itemId, quantity - 1);
        }

        public static void Clear()
        {
            foreach (var itemId in RestaurantStorage.LoadCart().Keys)
                RestaurantStorage.SetQuantity(itemId, 0);
        }
    }
}
