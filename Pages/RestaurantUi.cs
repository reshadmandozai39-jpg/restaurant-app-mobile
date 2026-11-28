using Microsoft.Maui.Controls.Shapes;

namespace NET_MAUI_App.Pages;

internal static class RestaurantUi
{
    public static readonly Color Blue = Color.FromArgb("#2563EB");
    public static readonly Color Ink = Color.FromArgb("#172033");
    public static readonly Color Muted = Color.FromArgb("#64748B");
    public static readonly Color Background = Color.FromArgb("#F5F7FB");
    public static readonly Color DarkSurface = Color.FromArgb("#1E293B");
    public static readonly Color DarkBackground = Color.FromArgb("#0F172A");
    public static readonly Color DarkInk = Color.FromArgb("#F1F5F9");
    public static readonly Color DarkMuted = Color.FromArgb("#CBD5E1");

    public static void PageTheme(ContentPage page) => page.SetAppThemeColor(VisualElement.BackgroundColorProperty, Colors.White, DarkBackground);
    public static void TextTheme(Label label) => label.SetAppThemeColor(Label.TextColorProperty, Ink, DarkInk);
    public static void SurfaceTheme(VisualElement view) => view.SetAppThemeColor(VisualElement.BackgroundColorProperty, Colors.White, DarkSurface);

    public static Label Heading(string text, double size = 22)
    {
        var label = new Label { Text = text, FontSize = size, FontAttributes = FontAttributes.Bold };
        TextTheme(label);
        return label;
    }

    public static Label Caption(string text)
    {
        var label = new Label { Text = text, FontSize = 13 };
        label.SetAppThemeColor(Label.TextColorProperty, Muted, DarkMuted);
        return label;
    }

    public static Button PrimaryButton(string text) => new()
    {
        Text = text, BackgroundColor = Blue, TextColor = Colors.White,
        FontSize = 15, FontAttributes = FontAttributes.Bold, CornerRadius = 8,
        MinimumHeightRequest = 48
    };

    public static Border Card(View content)
    {
        var card = new Border
        {
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Padding = 16,
            Content = content
        };
        SurfaceTheme(card);
        return card;
    }

    public static ScrollView PageContent(View content) => new()
    {
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(18, 20, 18, 28),
            MaximumWidthRequest = 600,
            HorizontalOptions = LayoutOptions.Fill,
            Children = { content }
        }
    };
}
