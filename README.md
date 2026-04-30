## 🏫 Warum diese App?

Stell dir folgendes vor:
 > Es ist Montagmorgen. Lisa ist Schülerin in der 10. Klasse. Sie öffnet ihre Schultasche und findet drei zerknitterte Zettel – Hausaufgaben von verschiedenen Lehrern, alle mit unterschiedlichen Deadlines. Sie weiß nicht mehr, was zuerst fällig ist. Ihr Mathelehrer hat die Aufgaben mündlich erklärt, ihr Biologielehrer hat sie per WhatsApp geschickt, und ihr Deutschlehrer hat sie an die Tafel geschrieben – die Lisa vergessen hat abzuschreiben.

> Gleichzeitig sitzt Herr Müller, ihr Klassenlehrer, im Lehrerzimmer und fragt sich: „Wer hat eigentlich das Referat abgegeben? Wer nicht?" Er blättert durch einen Stapel Papier und findet keine klare Antwort.

### Das Problem
- Aufgaben kommen per Zettel, WhatsApp und mündlich
- Schüler vergessen Deadlines
- Lehrer wissen nicht wer abgegeben hat
- Keine strukturierte Lernplanung
- Keine Motivation zum Lernen

# Unser Lösung : StudyMaster Pro 📚
Eine Desktop-Anwendung für Schüler und Lehrer zum Verwalten von Aufgaben, Lernzeiten und Klassen – entwickelt mit C# · WPF · SQL Server · .NET 8.

# 👥 Team
   => Amine Kansra
   => Aymane Alshian
   => Younes EL Hadoury
   => Saad Akki
   
# 🎯 Ziel der App
StudyMaster Pro löst folgende Probleme:

Schüler vergessen Aufgaben und Deadlines

Keine strukturierte Lernplanung vorhanden

Lehrer können Aufgaben nicht digital zuweisen

Kein Überblick über den eigenen Lernfortschritt

# 🌟 Features
 -- 👤 Benutzerverwaltung
 
 -- 📋 Aufgabenverwaltung
 
 -- 🏫 Klassen-System (wie Microsoft Teams)
 
 -- 📅 Lernplaner & Kalender
 
 -- 📊 Statistik & Fortschritt
 
 -- 🔔 Benachrichtigungen
 
 -- 🏆 Gamification
 
 -- 📁 Export & Backup
 
 -- Design

 ## 🛠️ Technologien
 
| Bereich | Technologie |
|---------|-------------|
| Sprache | C# (.NET 8) |
| UI | WPF · Material Design |
| Datenbank | SQL Server / SQL Server Express |
| ORM | Entity Framework Core 8 |
| Passwort | BCrypt.Net-Next |
| Diagramme | LiveCharts2 |
| PDF Export | PdfSharp |
| Excel Export | EPPlus |
| IDE | Visual Studio 2022 |
| Diagramm-Tool | Visual Paradigm |
 
---

# 🗄️ Datenbankstruktur
Users           → Benutzer (Admin / Lehrer / Schüler)

Tasks           → Aufgaben (mit Deadline, Priorität, Status)

SubTasks        → Unteraufgaben

StudySessions   → Lernsitzungen (Zeittracking)

Classes         → Klassen (Lehrer erstellt, Schüler tritt bei)

ClassMembers    → Wer ist in welcher Klasse

Submissions     → Abgaben der Schüler + Noten

Tags            → Etiketten für Aufgaben

TaskTags        → Verbindung Aufgabe ↔ Tag

Badges          → Abzeichen-Definitionen

UserBadges      → Welcher Schüler hat welches Badge verdient

Notifications   → Benachrichtigungen & Erinnerungen

## 🗺️ Entwicklungsplan
 
| Phase | Schritt | Status |
|-------|---------|--------|
| 1 | UML & Klassendiagramm | ✅ Fertig |
| 2 | SQL Datenbank erstellen |  Geplant |
| 3 | C# Projektstruktur |  Geplant |
| 4 | Models & Datenbankverbindung |  Geplant |
| 5 | Login & Benutzerverwaltung |  Geplant |
| 6 | Aufgaben & Klassen-System |  Geplant |
| 7 | Kalender & Benachrichtigungen |  Geplant |
| 8 | Statistik, Gamification & Export |  Geplant |
 
---
## 🔐 Rollen & Berechtigungen
 
| Funktion | Admin | Lehrer | Schüler |
|----------|:-----:|:------:|:-------:|
| Benutzer verwalten | ✅ | ❌ | ❌ |
| Klasse erstellen | ✅ | ✅ | ❌ |
| Klasse beitreten | ❌ | ❌ | ✅ |
| Aufgaben zuweisen | ✅ | ✅ | ❌ |
| Aufgaben abgeben | ❌ | ❌ | ✅ |
| Noten vergeben | ✅ | ✅ | ❌ |
| Gamification nutzen | ❌ | ❌ | ✅ |
| Statistik (gesamt) | ✅ | ✅ | ❌ |
| Export | ✅ | ✅ | ✅ |
 
---

## 📄 Lizenz
 
Dieses Projekt wurde im Rahmen eines Schulprojekts entwickelt.