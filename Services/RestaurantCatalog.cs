namespace NET_MAUI_App.Services;

public enum RestaurantMenuCategory { Tagesmenue, Vegi, Kinder }

public sealed record RestaurantMenuItem(string Id, string Name, string Description, decimal Price, string Emoji, RestaurantMenuCategory Category);

public static class RestaurantCatalog
{
    public static IReadOnlyList<RestaurantMenuItem> Menu { get; } = (RestaurantMenuItem[])
    [
        new("tagesmenue", "Tagesmenü", "Mit Beilage und Salat", 18.50m, "🍽️", RestaurantMenuCategory.Tagesmenue),
        new("tages-poulet", "Poulet mit Reis", "Mit Gemüse", 19.50m, "🍗", RestaurantMenuCategory.Tagesmenue),
        new("tages-fisch", "Fischfilet", "Mit Kartoffeln und Gemüse", 21.50m, "🐟", RestaurantMenuCategory.Tagesmenue),
        new("tages-pasta", "Pasta Bolognese", "Mit Tomatensauce", 18.50m, "🍝", RestaurantMenuCategory.Tagesmenue),
        new("tages-rind", "Rindsgeschnetzeltes", "Mit Reis", 22.50m, "🍽️", RestaurantMenuCategory.Tagesmenue),
        new("tages-curry", "Poulet-Curry", "Mit Reis", 19.50m, "🍛", RestaurantMenuCategory.Tagesmenue),
        new("tages-schnitzel", "Schnitzel", "Mit Kartoffeln", 20.50m, "🍽️", RestaurantMenuCategory.Tagesmenue),
        new("tages-lasagne", "Lasagne", "Mit Salat", 19.00m, "🍽️", RestaurantMenuCategory.Tagesmenue),
        new("vegi-menue", "Vegi-Menü", "Gemüse, Beilage und Salat", 16.50m, "🥗", RestaurantMenuCategory.Vegi),
        new("vegi-pasta", "Gemüse-Pasta", "Mit Tomatensauce", 16.50m, "🍝", RestaurantMenuCategory.Vegi),
        new("vegi-curry", "Gemüse-Curry", "Mit Reis", 17.50m, "🍛", RestaurantMenuCategory.Vegi),
        new("vegi-risotto", "Pilz-Risotto", "Mit Parmesan", 18.00m, "🍚", RestaurantMenuCategory.Vegi),
        new("vegi-lasagne", "Gemüse-Lasagne", "Mit Salat", 17.50m, "🍽️", RestaurantMenuCategory.Vegi),
        new("vegi-bowl", "Gemüse-Bowl", "Mit Reis und Gemüse", 16.50m, "🥗", RestaurantMenuCategory.Vegi),
        new("vegi-roesti", "Käse-Rösti", "Mit Salat", 18.00m, "🍽️", RestaurantMenuCategory.Vegi),
        new("vegi-suppe", "Gemüsesuppe", "Mit Brot und Salat", 15.00m, "🥣", RestaurantMenuCategory.Vegi),
        new("kinder-pasta", "Kinder-Pasta", "Mit Tomatensauce", 9.50m, "🍝", RestaurantMenuCategory.Kinder),
        new("kinder-poulet", "Kinder-Poulet", "Mit Reis", 11.50m, "🍗", RestaurantMenuCategory.Kinder),
        new("kinder-fisch", "Kinder-Fisch", "Mit Kartoffeln", 12.00m, "🐟", RestaurantMenuCategory.Kinder),
        new("kinder-ravioli", "Kinder-Ravioli", "Mit Tomatensauce", 10.00m, "🍝", RestaurantMenuCategory.Kinder),
        new("kinder-reis", "Kinder-Gemüsereis", "Mit saisonalem Gemüse", 9.50m, "🍚", RestaurantMenuCategory.Kinder),
        new("kinder-schnitzel", "Kinder-Schnitzel", "Mit Kartoffeln", 12.50m, "🍽️", RestaurantMenuCategory.Kinder),
        new("kinder-lasagne", "Kinder-Lasagne", "Kleine Portion", 11.00m, "🍽️", RestaurantMenuCategory.Kinder),
        new("kinder-suppe", "Kinder-Gemüsesuppe", "Mit Brot", 8.50m, "🥣", RestaurantMenuCategory.Kinder)
    ];
}
