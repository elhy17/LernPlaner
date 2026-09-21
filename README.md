# StudyMaster Pro

Digitale Schulorganisation für Schüler, Lehrer und Administratoren — WPF-Desktop-Anwendung (.NET 8) mit MySQL/MariaDB-Anbindung.

**Team Nexus:** Younes El Hadoury · Amine Kansra · Ayman Alshian · Saad Akki

## Voraussetzungen

- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 (mit Workload ".NET Desktop-Entwicklung") oder die `dotnet` CLI
- Zugriff auf eine MySQL/MariaDB-Datenbank

## Projekt starten

### 1. Repository klonen

```bash
git clone <repo-url>
cd Nexus-Team
```

### 2. Datenbankverbindung konfigurieren

Die Anwendung liest die Verbindungszeichenfolge in dieser Reihenfolge (siehe `App_LernPlaner/Database/db_verbindung.cs`):

1. Umgebungsvariable `STUDYMASTER_DB`
2. Falls nicht gesetzt: `App.config` → Connection String `StudyMasterDb`

**Format der Connection String:**

```
server=<HOST>;uid=<BENUTZER>;pwd=<PASSWORT>;database=<DATENBANKNAME>;
```

**Option A — über `App.config` (Standard, bereits im Repo hinterlegt):**

Die Datei [`App_LernPlaner/App.config`](App_LernPlaner/App.config) enthält bereits einen funktionierenden Eintrag `StudyMasterDb` für die Projekt-Datenbank. Für den Betrieb mit einer eigenen/lokalen Datenbank dort einfach die Werte anpassen:

```xml
<connectionStrings>
  <add name="StudyMasterDb"
       connectionString="server=HOST;uid=BENUTZER;pwd=PASSWORT;database=DATENBANKNAME;"
       providerName="MySql.Data.MySqlClient" />
</connectionStrings>
```

**Option B — über Umgebungsvariable (überschreibt `App.config`, z. B. für CI/andere Rechner):**

```powershell
setx STUDYMASTER_DB "server=HOST;uid=BENUTZER;pwd=PASSWORT;database=DATENBANKNAME;"
```

Terminal danach neu starten, damit die Variable aktiv wird.

### 3. Datenbankschema einspielen

Die Tabellen müssen einmalig angelegt werden. Additive Migrations-Skripte liegen unter [`App_LernPlaner/Database/migrations/`](App_LernPlaner/Database/migrations/) und werden der Reihe nach (nach Dateidatum sortiert) auf die Ziel-Datenbank ausgeführt, z. B.:

```bash
mysql -h <HOST> -u <BENUTZER> -p <DATENBANKNAME> < App_LernPlaner/Database/migrations/2026-08-28_add_missing_columns_and_messages.sql
```

Falls die Datenbank komplett leer ist, muss vorher zusätzlich das Basis-Schema (Tabellen `Users`, `Classes`, `ClassMembers`, `Tasks`, `SubTasks`, `Tags`, `TaskTags`, `Submissions`, `StudySessions`, `Notifications`, `Messages`, `Badges`, `UserBadge`) vorhanden sein, bevor die additiven Migrationsskripte angewendet werden.

### 4. Anwendung bauen und starten

Über die `dotnet` CLI:

```bash
dotnet restore
dotnet build App_LernPlaner.sln
dotnet run --project App_LernPlaner/App_LernPlaner.csproj
```

Oder in Visual Studio: `App_LernPlaner.sln` öffnen, `App_LernPlaner` als Startprojekt setzen und **F5** drücken.

### 5. Anmelden

Neue Benutzerkonten werden ausschließlich von einem Administrator angelegt (keine freie Selbstregistrierung). Für den ersten Login wird daher ein bereits in der Datenbank vorhandenes Administrator-Konto benötigt.

## Projektstruktur (Kurzüberblick)

```
App_LernPlaner/
├── App.config                  # Datenbank-Connection-String
├── Database/
│   ├── db_verbindung.cs        # Verbindungsaufbau (Env-Var → App.config)
│   └── migrations/             # Additive SQL-Migrationsskripte
├── Models/                     # Datenmodelle
├── Repositories/                # Einzige Schicht mit direktem DB-Zugriff (ADO.NET, parametrisiert)
├── Services/                    # Fachlogik
├── ViewModels/                  # MVVM-ViewModels
└── Views/                       # WPF-Seiten (Admin/, Chat/, Classes/, Gamification/, Statistics/, Tasks/, Teacher/, …)
```
