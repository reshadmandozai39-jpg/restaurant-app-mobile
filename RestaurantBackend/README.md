# Stripe-Test-Backend

Dieses separate ASP.NET-Core-Projekt ist nur für lokale Tests mit Stripe-Testschlüsseln. Es akzeptiert keine Live-Schlüssel und ist nicht für das Internet oder den produktiven Einsatz geeignet.

## Starten

1. Im Stripe-Dashboard den Testmodus aktivieren und den geheimen Testschlüssel kopieren (`sk_test_...`). Nicht in Dateien, die App oder den Chat einfügen.
2. In einem Terminal im Projektordner den Schlüssel mit .NET User Secrets setzen. Den Platzhalter durch den eigenen Testschlüssel ersetzen:

   ```powershell
   dotnet user-secrets set --project .\RestaurantBackend\RestaurantBackend.csproj "Stripe:SecretKey" "DEIN_TESTSCHLUESSEL"
   ```

   User Secrets liegen außerhalb des Projektordners, sind aber nicht verschlüsselt. Beachte, dass der eingegebene Befehl in der Terminal-Historie stehen kann. Für echte Deployments einen sicheren Secret Store verwenden.
3. Backend im Entwicklungsmodus starten, damit User Secrets geladen werden:

   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = 'Development'
   dotnet run --project .\RestaurantBackend\RestaurantBackend.csproj
   ```

   Es lauscht ausschließlich lokal auf `http://127.0.0.1:5187`.
4. Beispielanfrage in einem zweiten PowerShell-Fenster:

   ```powershell
   $body = @{ requestId = [guid]::NewGuid().ToString(); persons = 2; tableNumber = 12; items = @(@{ id = 'tagesmenue'; quantity = 1 }, @{ id = 'vegi-menue'; quantity = 1 }) } | ConvertTo-Json -Depth 4
   Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:5187/payments/checkout' -ContentType 'application/json' -Body $body
   ```

Im Backend wird der Betrag anhand des Serverkatalogs berechnet; die App öffnet Stripe Checkout im Browser. Unter Android versucht die Rückkehrseite, die App automatisch wieder zu öffnen. Falls der Browser das blockiert, auf „App öffnen“ tippen oder in der App „Zahlung prüfen“ auswählen. Unter Windows muss der Status manuell geprüft werden. Der Rücksprung allein ist kein Zahlungsnachweis: Die App fragt Status, Betrag, Zahlungsmethode und Bestellpositionen bei Stripe ab und meldet Erfolg nur bei `paid`. Bestellpositionen und Preise werden direkt aus Stripes Checkout-Line-Items abgerufen. Die App speichert nach bestätigter Zahlung nur die Checkout-ID sicher auf dem Gerät; Bestellverlauf und Ausgaben werden bei Stripe verifiziert und ohne eigene Bestelldatenbank berechnet. Der Checkout-Link sollte nicht geteilt werden.

Die Preise kommen ausschließlich aus `Services\RestaurantCatalog.cs`, nicht aus der Anfrage. Der Katalog enthält momentan Beispielgerichte und Beispielpreise; für ein echtes Restaurant müssen diese ersetzt und auf dem Server gepflegt werden.

## Docker

Das Image wird vom Repository-Hauptverzeichnis aus gebaut, da das Backend den gemeinsamen Katalog benötigt:

```powershell
docker build -t restaurant-backend .
```

Für das Hosting die Dockerfile im Repository-Hauptverzeichnis und dieses Verzeichnis als Build-Kontext auswählen. Das Image lauscht standardmäßig auf Port `10000`; falls der Anbieter einen anderen Port vorgibt, `ASPNETCORE_URLS` entsprechend setzen. Im Hosting als Umgebungsvariablen `Stripe__SecretKey` mit dem geheimen Stripe-Testschlüssel und `PublicBaseUrl` mit der öffentlichen HTTPS-Adresse des Backends setzen. Geheimnisse nicht in die Dockerfile oder ins Repository schreiben. Keine lokale Development-Umgebung im Container aktivieren, da das Backend in diesem Modus nur auf `127.0.0.1:5187` lauscht.

## Vor echtem Betrieb erforderlich

Dieses Backend ist ausschließlich für lokale Entwicklung und Stripe-Testzahlungen vorgesehen. Für Android/iOS wird eine erreichbare HTTPS-Adresse benötigt. Das Backend ist jedoch noch nicht für die öffentliche Bereitstellung abgesichert; vor einer Veröffentlichung müssen Authentifizierung und Schutz vor Missbrauch ergänzt werden. In der App öffne **Konto → Backend für Android**, trage die HTTPS-Adresse ein und speichere sie. Checkout fordert Karte und TWINT an; Stripe zeigt TWINT nur, wenn es für das Konto und den Testmodus aktiviert und verfügbar ist.

Für Produktivbetrieb fehlen außerdem ein verifizierter Stripe-Webhook, Authentifizierung und serverseitige Bestellübermittlung an das Restaurant. Der Verlauf ist nur auf dem Gerät verfügbar, auf dem die Checkout-IDs sicher gespeichert wurden; nach App-Neuinstallation oder Gerätewechsel kann er nicht wiederhergestellt werden. Niemals Live-Schlüssel oder Test-Checkout-Links veröffentlichen.
