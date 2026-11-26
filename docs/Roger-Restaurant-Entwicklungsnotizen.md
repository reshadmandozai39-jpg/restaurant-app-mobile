# Roger Restaurant – Android-App

## Aktueller Ablauf

1. App starten und mit E-Mail/Passwort anmelden.
2. Falls noch kein lokales Konto existiert, „Neues Konto erstellen“ wählen, Name, E-Mail und Passwort eingeben und danach anmelden.
3. Auf „Start“ begrüßt die App den Gast; über „Menü“ werden Tagesmenü und Vegi-Menü angezeigt.
4. „Hinzufügen“ legt ein Menü in den Warenkorb. Dort werden Menge und CHF-Gesamtsumme angezeigt.
5. Im Profil erscheinen der gespeicherte Name sowie Platzhalter für Besuche, Bestellungen und Ausgaben.

## Ordner und Dateien

```text
Pages/
  LoginPage.xaml(.cs)    Anmeldung
  RegisterPage.cs        Lokale Kontoerstellung
  HomePage.cs            Begrüßung und Einstieg
  MenuPage.cs            Beispielmenüs und Warenkorb-Aktion
  CartPage.cs            Warenkorb, Summe und Zahlungs-Auswahl
  ProfilePage.cs         Konto- und Treueprogramm-Übersicht
Services/
  LocalAccountService.cs Speichert Kontodaten und Passwort-Hash lokal geschützt
  RestaurantOrderService.cs Beispielmenüs und Warenkorbzustand
App.xaml.cs              Startet die Anmeldung
AppShell.xaml(.cs)       Restaurant-Navigation als Tabs
NET MAUI App.csproj       .NET-MAUI-Projekt und Plattformen
```

## Wichtige Grenzen des Prototyps

- Das Konto wird nur auf diesem Gerät gespeichert. Es ist kein Online-Konto.
- Menü und Warenkorb sind Beispieldaten; der Warenkorb wird derzeit nicht dauerhaft gespeichert.
- TWINT und Kreditkarte werden nur als Auswahl angezeigt. Es wird nichts belastet und es entsteht keine bezahlte Bestellung.
- Echte Zahlungen benötigen einen Zahlungsanbieter (zum Beispiel TWINT oder einen Kartenanbieter), ein sicheres Backend für die Zahlungsanforderung und Anbieter-Zugangsdaten. Geheime Schlüssel dürfen nicht in die Android-App eingebaut werden.
- Besuchszähler, Bestellhistorie und Ausgaben sind noch nicht mit bezahlten Bestellungen verbunden.

## Nächste Schritte

1. Farben, Logo, Bilder und Schrift gemäß Mockup gestalten.
2. Menüdaten und Bilder des Restaurants ergänzen.
3. Backend und Benutzerverwaltung auswählen, wenn Konten geräteübergreifend gebraucht werden.
4. Zahlungsanbieter und Backend einrichten; zunächst nur im Testmodus testen.
5. Erst nach bestätigter Zahlung eine Bestellung speichern und Treuebesuche gutschreiben.
6. Android-Emulator testen: Anmeldung, Menüs, Warenkorb, Profil und Fehlerfälle.
