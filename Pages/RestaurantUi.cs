using Microsoft.Maui.Controls.Shapes;

namespace NET_MAUI_App.Pages;

internal static class RestaurantUi
{
    public static readonly Color Blue = Color.FromArgb("#2563EB");
    public static readonly Color Ink = Color.FromArgb("#172033");
    public static readonly Color Muted = Color.FromArgb("#64748B");
    public static readonly Color Background = Color.FromArgb("#F5F7FB");

    public static Label Heading(string text, double size = 22) => new()
    {
        Text = text, FontSize = size, FontAttributes = FontAttributes.Bold, TextColor = Ink
    };

    public static Label Caption(string text) => new()
    {
        Text = text, FontSize = 13, TextColor = Muted
    };

    public static Button PrimaryButton(string text) => new()
    {
        Text = text, BackgroundColor = Blue, TextColor = Colors.White,
        FontSize = 15, FontAttributes = FontAttributes.Bold, CornerRadius = 8,
        MinimumHeightRequest = 48
    };

    public static Border Card(View content) => new()
    {
        BackgroundColor = Colors.White,
        Stroke = Color.FromArgb("#E2E8F0"),
        StrokeThickness = 1,
        StrokeShape = new RoundRectangle { CornerRadius = 10 },
        Padding = 16,
        Content = content
    };

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
