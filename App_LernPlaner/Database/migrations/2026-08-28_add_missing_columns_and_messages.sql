-- Additive schema migration for StudyMaster Pro (pbt3h24aas_Nexus)
-- Applied 2026-08-28. Adds columns/tables required by the Pflichtenheft and by
-- this feature batch (admin/teacher dashboards, chat, grading). Nothing is
-- dropped or overwritten - only new nullable/defaulted columns and new tables.

-- FA-08: jede Klasse braucht einen eindeutigen Einladungscode
ALTER TABLE Classes ADD COLUMN IF NOT EXISTS JoinCode VARCHAR(20) NULL;
ALTER TABLE Classes ADD COLUMN IF NOT EXISTS CreatedAt DATETIME NULL;
ALTER TABLE Classes ADD UNIQUE INDEX IF NOT EXISTS UX_Classes_JoinCode (JoinCode);

ALTER TABLE ClassMembers ADD COLUMN IF NOT EXISTS JoinedAt DATETIME NULL;

-- FA-20/FA-21: Note und Kommentar zu einer Abgabe
ALTER TABLE Submissions ADD COLUMN IF NOT EXISTS Grade VARCHAR(5) NULL;
ALTER TABLE Submissions ADD COLUMN IF NOT EXISTS Feedback TEXT NULL;

-- FA-05: Admin kann Benutzerkonten deaktivieren
ALTER TABLE Users ADD COLUMN IF NOT EXISTS IsActive TINYINT(1) NOT NULL DEFAULT 1;

-- Diese Primärschlüssel hatten in der ursprünglichen DB kein AUTO_INCREMENT,
-- wodurch jedes INSERT fehlgeschlagen wäre.
ALTER TABLE Classes MODIFY ClassId INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE ClassMembers MODIFY Id INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE Notifications MODIFY NotifId INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE Badges MODIFY BadgeId INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE StudySessions MODIFY SessionId INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE SubTasks MODIFY SubTaskId INT(11) NOT NULL AUTO_INCREMENT;
ALTER TABLE Tags MODIFY TagId INT(11) NOT NULL AUTO_INCREMENT;

-- Statische Referenzdaten für das Gamification-System (FA-30..32, Pflichtenheft 6.9)
INSERT INTO Badges (Name, Description, IconPath, XPReward)
SELECT * FROM (SELECT 'Erste Abgabe', 'Erste Aufgabe erfolgreich abgegeben', '🥇', 20) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Badges WHERE Name = 'Erste Abgabe');
INSERT INTO Badges (Name, Description, IconPath, XPReward)
SELECT * FROM (SELECT 'Fleißiger Schüler', '5 Aufgaben pünktlich abgegeben', '🔥', 50) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Badges WHERE Name = 'Fleißiger Schüler');
INSERT INTO Badges (Name, Description, IconPath, XPReward)
SELECT * FROM (SELECT 'Streber', 'Note 1 erhalten', '⭐', 50) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Badges WHERE Name = 'Streber');
INSERT INTO Badges (Name, Description, IconPath, XPReward)
SELECT * FROM (SELECT 'Lernmarathon', '10 Stunden Lernzeit erfasst', '📚', 30) AS tmp
WHERE NOT EXISTS (SELECT 1 FROM Badges WHERE Name = 'Lernmarathon');

-- Chat: jeder Benutzer kann jedem Benutzer schreiben (Nachrichten, editierbar/löschbar)
CREATE TABLE IF NOT EXISTS Messages (
  MessageId INT(11) NOT NULL AUTO_INCREMENT,
  SenderId INT(11) NOT NULL,
  ReceiverId INT(11) NOT NULL,
  Body TEXT NOT NULL,
  SentAt DATETIME NOT NULL,
  EditedAt DATETIME NULL,
  IsDeleted TINYINT(1) NOT NULL DEFAULT 0,
  IsRead TINYINT(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (MessageId),
  KEY SenderId (SenderId),
  KEY ReceiverId (ReceiverId),
  CONSTRAINT Messages_ibfk_1 FOREIGN KEY (SenderId) REFERENCES Users (UserId),
  CONSTRAINT Messages_ibfk_2 FOREIGN KEY (ReceiverId) REFERENCES Users (UserId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;
