namespace NET_MAUI_App.Pages;

public class FaqPage : ContentPage
{
    public FaqPage()
    {
        Title = "Häufige Fragen";
        BackgroundColor = RestaurantUi.Background;
        var questions = new VerticalStackLayout { Spacing = 10 };
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
            Children = { RestaurantUi.Heading("Häufige Fragen"), questions }
        });

        void Add(string question, string answer)
        {
            questions.Children.Add(RestaurantUi.Card(new VerticalStackLayout
            {
                Spacing = 6,
                Children = { RestaurantUi.Heading(question, 16), RestaurantUi.Caption(answer) }
            }));
        }
    }
}
