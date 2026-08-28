using System;
using System.Collections.Generic;
using System.Linq;
using App_LernPlaner.Models;

namespace App_LernPlaner.Data
{
    /// <summary>
    /// In-Memory Ersatz für die echte MySQL-Datenbank, solange keine DB-Verbindung
    /// besteht (siehe AppConfig.UseMockData). Enthält realistische Beispieldaten
    /// für alle Rollen (Admin, Lehrer, Schüler), damit die komplette App ohne
    /// Datenbank getestet werden kann.
    /// </summary>
    public static class MockDatabase
    {
        public static List<User> Users { get; } = new();
        public static List<Class> Classes { get; } = new();
        public static List<ClassMember> ClassMembers { get; } = new();
        public static List<StudyTask> Tasks { get; } = new();
        public static List<Submission> Submissions { get; } = new();
        public static List<Notification> Notifications { get; } = new();
        public static List<Badge> Badges { get; } = new();
        public static List<UserBadge> UserBadges { get; } = new();
        public static List<StudySession> StudySessions { get; } = new();

        private static int _nextUserId = 1;
        private static int _nextClassId = 1;
        private static int _nextTaskId = 1;
        private static int _nextSubmissionId = 1;
        private static int _nextNotificationId = 1;
        private static int _nextBadgeId = 1;
        private static int _nextStudySessionId = 1;

        public static int NextUserId() => _nextUserId++;
        public static int NextClassId() => _nextClassId++;
        public static int NextTaskId() => _nextTaskId++;
        public static int NextSubmissionId() => _nextSubmissionId++;
        public static int NextNotificationId() => _nextNotificationId++;
        public static int NextStudySessionId() => _nextStudySessionId++;

        static MockDatabase()
        {
            Seed();
        }

        /// <summary>Standard-Passwort für alle Mock-Benutzer: "Passwort123"</summary>
        private static void Seed()
        {
            string demoHash = BCrypt.Net.BCrypt.HashPassword("Passwort123");

            var admin = new User { UserId = NextUserId(), UserName = "admin", Email = "admin@studymaster.de", PasswordHash = demoHash, Role = Roles.Admin, XPPoints = 0, CreatedAt = DateTime.Now.AddMonths(-6) };
            var mueller = new User { UserId = NextUserId(), UserName = "mueller", Email = "mueller@studymaster.de", PasswordHash = demoHash, Role = Roles.Lehrer, XPPoints = 0, CreatedAt = DateTime.Now.AddMonths(-5) };
            var schmidtL = new User { UserId = NextUserId(), UserName = "schmidt", Email = "schmidt@studymaster.de", PasswordHash = demoHash, Role = Roles.Lehrer, XPPoints = 0, CreatedAt = DateTime.Now.AddMonths(-5) };
            var koch = new User { UserId = NextUserId(), UserName = "koch", Email = "koch@studymaster.de", PasswordHash = demoHash, Role = Roles.Lehrer, XPPoints = 0, CreatedAt = DateTime.Now.AddMonths(-5) };

            var max = new User { UserId = NextUserId(), UserName = "max", Email = "max@studymaster.de", PasswordHash = demoHash, Role = Roles.Schueler, XPPoints = 680, CreatedAt = DateTime.Now.AddMonths(-3) };
            var lisa = new User { UserId = NextUserId(), UserName = "lisa", Email = "lisa@studymaster.de", PasswordHash = demoHash, Role = Roles.Schueler, XPPoints = 910, CreatedAt = DateTime.Now.AddMonths(-3) };
            var tom = new User { UserId = NextUserId(), UserName = "tom", Email = "tom@studymaster.de", PasswordHash = demoHash, Role = Roles.Schueler, XPPoints = 340, CreatedAt = DateTime.Now.AddMonths(-2) };
            var mia = new User { UserId = NextUserId(), UserName = "mia", Email = "mia@studymaster.de", PasswordHash = demoHash, Role = Roles.Schueler, XPPoints = 520, CreatedAt = DateTime.Now.AddMonths(-2) };

            Users.AddRange(new[] { admin, mueller, schmidtL, koch, max, lisa, tom, mia });

            var mathe = new Class { ClassId = NextClassId(), Name = "Mathematik 10a", TeacherId = mueller.UserId, JoinCode = "MATH10A", CreatedAt = DateTime.Now.AddMonths(-4) };
            var datenbanken = new Class { ClassId = NextClassId(), Name = "Datenbanken 11b", TeacherId = schmidtL.UserId, JoinCode = "SQL11B", CreatedAt = DateTime.Now.AddMonths(-4) };
            var englisch = new Class { ClassId = NextClassId(), Name = "Englisch 9c", TeacherId = koch.UserId, JoinCode = "ENG9C", CreatedAt = DateTime.Now.AddMonths(-4) };
            var programmierung = new Class { ClassId = NextClassId(), Name = "Programmierung 12a", TeacherId = mueller.UserId, JoinCode = "PROG12A", CreatedAt = DateTime.Now.AddMonths(-3) };

            Classes.AddRange(new[] { mathe, datenbanken, englisch, programmierung });

            void Join(Class c, User u, byte role)
            {
                ClassMembers.Add(new ClassMember { ClassId = c.ClassId, UserId = u.UserId, Role = role, JoinedAt = DateTime.Now.AddMonths(-2) });
            }

            const byte teacherRole = 1;
            const byte studentRole = 2;

            Join(mathe, mueller, teacherRole);
            Join(datenbanken, schmidtL, teacherRole);
            Join(englisch, koch, teacherRole);
            Join(programmierung, mueller, teacherRole);

            foreach (var s in new[] { max, lisa, tom, mia })
            {
                Join(mathe, s, studentRole);
                Join(datenbanken, s, studentRole);
                Join(englisch, s, studentRole);
            }
            Join(programmierung, max, studentRole);
            Join(programmierung, lisa, studentRole);

            StudyTask AddTask(string title, string subject, Class cls, DateTime deadline, byte prio, byte status, User createdBy, bool recurring = false)
            {
                var t = new StudyTask
                {
                    TaskId = NextTaskId(),
                    UserId = createdBy.UserId,
                    ClassId = cls.ClassId,
                    CreatedBy = createdBy.UserId,
                    Titel = title,
                    Subject = subject,
                    Deadline = deadline,
                    Priority = prio,
                    Status = status,
                    IsRecurring = recurring,
                    ButtonText = "Öffnen"
                };
                Tasks.Add(t);
                return t;
            }

            var mathTask = AddTask("Mathe Hausaufgaben Kapitel 3", "Mathematik", mathe, DateTime.Today.AddDays(-2), 3, 0, mueller);
            AddTask("SQL Übungsblatt", "Datenbanken", datenbanken, DateTime.Today.AddDays(-1), 3, 0, schmidtL);
            AddTask("C# Projekt fertigstellen", "Programmierung", programmierung, DateTime.Today.AddDays(5), 2, 1, mueller);
            var vokabelnTask = AddTask("Englisch Vokabeln lernen", "Englisch", englisch, DateTime.Today.AddDays(-10), 1, 2, koch);
            AddTask("Geometrie Übungen", "Mathematik", mathe, DateTime.Today.AddDays(3), 2, 0, mueller);
            AddTask("Datenbank ER-Modell entwerfen", "Datenbanken", datenbanken, DateTime.Today.AddDays(7), 2, 0, schmidtL);

            Submissions.Add(new Submission
            {
                SubmissionId = NextSubmissionId(),
                UserId = max.UserId,
                TaskId = vokabelnTask.TaskId,
                Status = 2,
                Abgabe = "Alle 50 Vokabeln in der Anlage gelernt und geübt.",
                FilePath = "",
                Grade = "2",
                Feedback = "Gut gemacht, weiter so!",
                SubmittedAt = DateTime.Now.AddDays(-9)
            });

            Notifications.Add(new Notification { NotifId = NextNotificationId(), UserId = max.UserId, Message = "Neue Aufgabe: Mathe Hausaufgaben Kapitel 3", TriggerAt = DateTime.Now.AddDays(-2), IsRead = false });
            Notifications.Add(new Notification { NotifId = NextNotificationId(), UserId = max.UserId, Message = "Du hast eine Bewertung erhalten: Englisch Vokabeln lernen (Note 2)", TriggerAt = DateTime.Now.AddDays(-8), IsRead = false });
            Notifications.Add(new Notification { NotifId = NextNotificationId(), UserId = max.UserId, Message = "Deadline in 24h: SQL Übungsblatt", TriggerAt = DateTime.Now.AddHours(-3), IsRead = false });

            Badges.AddRange(new[]
            {
                new Badge { BadgeId = _nextBadgeId++, Name = "Erste Abgabe", Description = "Erste Aufgabe erfolgreich abgegeben", IconPath = "🥇", XPReward = 20 },
                new Badge { BadgeId = _nextBadgeId++, Name = "Fleißiger Schüler", Description = "5 Aufgaben pünktlich abgegeben", IconPath = "🔥", XPReward = 50 },
                new Badge { BadgeId = _nextBadgeId++, Name = "Streber", Description = "Note 1 erhalten", IconPath = "⭐", XPReward = 50 },
                new Badge { BadgeId = _nextBadgeId++, Name = "Lernmarathon", Description = "10 Stunden Lernzeit erfasst", IconPath = "📚", XPReward = 30 },
            });

            UserBadges.Add(new UserBadge { UserId = max.UserId, BadgeId = Badges[0].BadgeId, DateEarned = DateTime.Now.AddDays(-8), Badge = Badges[0] });

            StudySessions.Add(new StudySession { SessionId = NextStudySessionId(), UserId = max.UserId, TaskId = (uint)mathTask.TaskId, StartTime = DateTime.Now.AddDays(-1).AddHours(-2), DurationMin = 90, Notes = "Kapitel 3 wiederholt" });
            StudySessions.Add(new StudySession { SessionId = NextStudySessionId(), UserId = max.UserId, TaskId = 0, StartTime = DateTime.Now.AddDays(-3), DurationMin = 45, Notes = "Vokabeln geübt" });
        }

        public static string GenerateJoinCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            string code;
            do
            {
                code = new string(Enumerable.Range(0, 7).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
            } while (Classes.Any(c => c.JoinCode == code));
            return code;
        }
    }
}
