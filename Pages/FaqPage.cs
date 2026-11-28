using Microsoft.Maui.Controls.Shapes;

namespace NET_MAUI_App.Pages;

public class FaqPage : ContentPage
{
    public FaqPage()
    {
        Title = "Häufige Fragen";
        BackgroundColor = RestaurantUi.Background;
        var questions = new VerticalStackLayout { Spacing = 8 };
        Add("Kann ich bar bezahlen?", "Nein. Stripe Checkout fordert Karte und TWINT an, sofern diese für dein Stripe-Konto im Testmodus verfügbar sind. Es werden nur Testzahlungen verarbeitet.");
        Add("Wie viele Personen pro Tisch?", "Pro Tisch sind maximal 8 Personen vorgesehen.");
        Add("Wo finde ich die Tischnummer?", "Die Tischnummer steht auf deinem Tisch.");
        Add("Wo sehe ich meine Bestellungen?", "Unter Konto findest du lokal gespeicherte Entwürfe. Es werden noch keine Bestellungen übermittelt.");
        Add("Kann ich meine Auswahl ändern?", "Ja. Im Menü kannst du Mengen ändern, bevor du den Entwurf speicherst.");
        Add("Gibt es Menüs für Kinder?", "Ja, unter dem Filter Kinder. Die Menüs und Preise sind Beispiele.");
        Add("Wie funktioniert das Gratis-Menü?", "Das Bonusprogramm ist noch nicht angebunden.");
        Add("Was passiert bei der Zahlung?", "Die App öffnet Stripe Checkout im Testmodus. Erst nach Bestätigung durch Stripe zeigt die App die Zahlung als erfolgreich an; echtes Geld wird nicht belastet.");
        Add("Wo werden meine Daten gespeichert?", "Konto, Warenkorb und gespeicherte Entwürfe liegen nur auf diesem Gerät.");
        Content = RestaurantUi.PageContent(new VerticalStackLayout
        {
            Spacing = 16,
            Children = { questions }
        });

        void Add(string question, string answer)
        {
            var preview = RestaurantUi.Caption(answer);
            preview.FontSize = 14;
            preview.TextColor = Color.FromArgb("#475569");
            preview.MaxLines = 2;
            preview.LineBreakMode = LineBreakMode.TailTruncation;

            var details = RestaurantUi.Caption(answer);
            details.FontSize = 14;
            details.TextColor = RestaurantUi.Ink;
            details.LineHeight = 1.25;

            var answerContainer = new Border
            {
                BackgroundColor = Color.FromArgb("#F8FAFC"),
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = 12,
                Margin = new Thickness(0, 4, 0, 0),
                Content = details,
                IsVisible = false
            };

            var indicator = new Label
            {
                Text = FluentUI.chevron_right_20_regular,
                FontFamily = FluentUI.FontFamily,
                FontSize = 20,
                TextColor = RestaurantUi.Muted,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };

            var header = new Grid
            {
                ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
                ColumnSpacing = 12
            };
            header.Add(new VerticalStackLayout
            {
                Spacing = 4,
                Children = { RestaurantUi.Heading(question, 16), preview }
            });
            header.Add(indicator, 1);

            var card = RestaurantUi.Card(new VerticalStackLayout
            {
                Spacing = 10,
                Children = { header, answerContainer }
            });
            card.Padding = 12;

            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) =>
            {
                answerContainer.IsVisible = !answerContainer.IsVisible;
                preview.IsVisible = !answerContainer.IsVisible;
                indicator.Text = answerContainer.IsVisible
                    ? FluentUI.chevron_down_20_regular
                    : FluentUI.chevron_right_20_regular;
            };
            header.GestureRecognizers.Add(tap);
            questions.Children.Add(card);
        }
    }
}
