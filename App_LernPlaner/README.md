# LernPlaner

Mit LernPlaner können Schülerinnen und Schüler ihre Aufgaben organisieren, Abgaben einreichen und Lernzeiten festhalten. Lehrkräfte erstellen Klassen und Aufgaben und geben Bewertungen und Feedback. Dazu kommen ein Kalender, Chat, Benachrichtigungen und Statistiken. XP und Abzeichen machen den eigenen Fortschritt sichtbar.

## Verwendete Technologien

Die Anwendung ist in **C# mit .NET 8** geschrieben. Die Oberfläche nutzt **WPF** und ist nach **MVVM** aufgebaut: Darstellung, Oberflächenlogik und Daten sind voneinander getrennt.

**Entity Framework Core** übernimmt das Lesen und Speichern der Daten in **Microsoft SQL Server**. Für den Zugriff von anderen Rechnern gibt es eine **ASP.NET Core API**, mit der die Desktopanwendung über **HTTPS** kommuniziert. Die Zugangsdaten zur Datenbank bleiben dabei auf dem Server.

Passwörter werden mit **BCrypt** gehasht. Nach der Anmeldung erhält die App einen zeitlich begrenzten Sitzungstoken. Die API prüft bei jeder Anfrage, welche Aktionen der angemeldete Benutzer ausführen darf. Die Rollen sind Schüler, Lehrer und Administrator.

MySQL wird in dieser Version nicht mehr unterstützt. Bestehende Daten aus MySQL werden nicht automatisch übernommen.

## Einrichtung

Du brauchst Windows, ein .NET SDK mit Unterstützung für .NET 8 sowie die passenden Laufzeitumgebungen für Windows Desktop und ASP.NET Core. Als Datenbank kannst du SQL Server ab Version 2019 verwenden. Für die lokale Entwicklung reicht SQL Server LocalDB.

Führe die folgenden Befehle in PowerShell im Projektordner aus.

### Projekt bauen

```powershell
dotnet restore LernPlaner.sln --configfile NuGet.Config --packages artifacts/packages
dotnet build LernPlaner.sln --no-restore
```

### Datenbank vorbereiten

Starte LocalDB und lege die Verbindung für die aktuelle Sitzung in PowerShell fest:

```powershell
SqlLocalDB start MSSQLLocalDB
$env:LERNPLANER_DB = 'Server=(localdb)\MSSQLLocalDB;Database=LernPlaner_Dev;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
dotnet run --project src/LernPlaner.Api/LernPlaner.Api.csproj --no-launch-profile -- --migrate
```

Der letzte Befehl erstellt die Datenbank und wendet die vorhandenen Migrationen von EF Core an. Danach beendet er sich. Bei späteren Änderungen an der Datenbankstruktur wird derselbe Befehl erneut ausgeführt. Beim normalen Start der App werden keine Migrationen angewendet.

Für einen anderen SQL Server ersetzt du die Verbindungszeichenfolge in `LERNPLANER_DB`. Außerhalb von LocalDB sind `Encrypt=True;TrustServerCertificate=False` und ein vertrauenswürdiges Serverzertifikat erforderlich. Zugangsdaten gehören in Umgebungsvariablen oder einen geschützten Secretspeicher, nicht in den Quellcode.

### Erstes Administratorkonto anlegen

Es gibt kein voreingestelltes Konto. Erstelle nach der Datenbankeinrichtung einen Administrator:

```powershell
$env:LERNPLANER_ADMIN_USER = 'admin'
$env:LERNPLANER_ADMIN_EMAIL = 'admin@example.test'
$env:LERNPLANER_ADMIN_PASSWORD = [System.Net.NetworkCredential]::new('', (Read-Host 'Passwort für den Administrator' -AsSecureString)).Password
dotnet run --project src/LernPlaner.Api/LernPlaner.Api.csproj --no-launch-profile -- --bootstrap-admin
Remove-Item Env:LERNPLANER_ADMIN_PASSWORD
```

Passe Benutzername und Mailadresse an. Der Befehl funktioniert nur, solange noch kein Administrator vorhanden ist. Weitere Konten für Schüler und Lehrer legst du anschließend über die Benutzerverwaltung der App an.

### API und Desktopanwendung starten

Vertraue zunächst dem lokalen Entwicklungszertifikat für HTTPS, falls das noch nicht eingerichtet ist:

```powershell
dotnet dev-certs https --trust
```

Starte die API in dem Fenster, in dem `LERNPLANER_DB` gesetzt ist:

```powershell
dotnet run --project src/LernPlaner.Api/LernPlaner.Api.csproj --launch-profile https
```

Öffne ein zweites Fenster in PowerShell im Projektordner und starte die Desktopanwendung:

```powershell
$env:LERNPLANER_API_URL = 'https://localhost:7243/'
dotnet run --project App_LernPlaner.csproj
```

Melde dich mit dem zuvor angelegten Administratorkonto an. Für einen entfernten Server ersetzt du die lokale Adresse der API durch die HTTPS Adresse des Servers. Auf dem Server müssen die Datenbankverbindung und ein gültiges Zertifikat für HTTPS eingerichtet sein. Die Desktopanwendung benötigt dort keine eigenen Datenbankzugangsdaten; der Port von SQL Server sollte nur vom Server der API aus erreichbar sein.

Für die lokale Entwicklung kannst du auch direkt auf die Datenbank zugreifen: Entferne dafür `LERNPLANER_API_URL`, setze `LERNPLANER_DB` im selben Fenster und starte WPF. Sobald eine Adresse für die API gesetzt ist, verwendet die App die API und wechselt bei Verbindungsproblemen nicht automatisch zur Datenbank.

## Tests

Mit diesem Befehl werden die Abhängigkeiten geladen, die Projekte gebaut und die Tests ausgeführt:

```powershell
./scripts/Verify.ps1
```

Die Datenbanktests verwenden standardmäßig LocalDB und erstellen eigene temporäre Datenbanken, die danach wieder gelöscht werden. Getestet werden unter anderem das Speichern und Laden von Daten, Anmeldung, Berechtigungen, den Zugriff über HTTPS und Teile der Anwendung mit WPF. Ohne verfügbaren SQL Server kannst du `./scripts/Verify.ps1 -SkipSql` verwenden; die Datenbanktests werden dann übersprungen.

## Aktuelle Einschränkungen

Die Passwortwiederherstellung per Mail ist noch nicht umgesetzt. Zurücksetzungen müssen deshalb sicher durch einen Administrator oder Betreiber erfolgen. Anhänge und Profilbilder werden bisher nur als Dateipfade gespeichert und nicht zwischen Rechnern übertragen.

Die API wurde lokal über HTTPS mit SQL Server getestet. Für den Einsatz auf einem öffentlichen Server sind noch die Einrichtung von Zertifikaten, Netzwerk und Firewall sowie Tests in der jeweiligen Umgebung nötig.
