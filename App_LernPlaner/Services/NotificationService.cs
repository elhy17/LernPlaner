using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Data;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    /// <summary>Benachrichtigungen im Dashboard (FA-22 bis FA-25).</summary>
    public class NotificationService
    {
        private const string DailyLoginTag = "__daily_login__";

        private readonly NotificationRepository _notifications = new();
        private readonly ClassService _classService = new();
        private readonly TaskRepository _tasks = new();

        public async Task<List<Notification>> GetForUserAsync(int userId)
        {
            var list = AppConfig.UseMockData
                ? MockDatabase.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.TriggerAt).ToList()
                : await _notifications.GetForUserAsync(userId);

            return list.Where(n => n.Message != DailyLoginTag).ToList();
        }

        public async Task<int> CountUnreadAsync(int userId)
        {
            if (AppConfig.UseMockData)
                return MockDatabase.Notifications.Count(n => n.UserId == userId && !n.IsRead && n.Message != DailyLoginTag);

            return await _notifications.CountUnreadAsync(userId);
        }

        public async Task NotifyAsync(int userId, string message)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current != null && userId != sessionManager.CurrentUserId) return;
            if (AppConfig.UseMockData)
            {
                MockDatabase.Notifications.Add(new Notification { NotifId = MockDatabase.NextNotificationId(), UserId = userId, Message = message, TriggerAt = DateTime.Now, IsRead = false });
                return;
            }

            await _notifications.InsertAsync(userId, message);
        }

        public async Task MarkAsReadAsync(int notifId)
        {
            if (AppConfig.UseMockData)
            {
                var n = MockDatabase.Notifications.FirstOrDefault(x => x.NotifId == notifId);
                if (n != null) n.IsRead = true;
                return;
            }

            await _notifications.MarkAsReadAsync(notifId);
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            if (AppConfig.UseMockData)
            {
                foreach (var n in MockDatabase.Notifications.Where(x => x.UserId == userId)) n.IsRead = true;
                return;
            }

            await _notifications.MarkAllAsReadAsync(userId);
        }

        // FA-23: Deadline-Erinnerung 24h vorher. Wird beim Dashboard-Laden geprüft,
        // da kein Hintergrund-Scheduler vorhanden ist.
        public async Task CheckDeadlineRemindersAsync(User user)
        {
            var myClassIds = (await _classService.GetClassesForUserAsync(user.UserId)).Select(c => c.ClassId).ToHashSet();

            var visibleTasks = AppConfig.UseMockData
                ? MockDatabase.Tasks.Where(t => t.ClassId.HasValue && myClassIds.Contains(t.ClassId.Value)).ToList()
                : await _tasks.GetVisibleTasksForUserAsync(user.UserId);

            var upcoming = visibleTasks.Where(t => t.Status != 2 && t.Deadline.Date == DateTime.Today.AddDays(1));

            foreach (var task in upcoming)
            {
                string prefix = $"Deadline in 24h: {task.Titel}";
                bool alreadyNotified = AppConfig.UseMockData
                    ? MockDatabase.Notifications.Any(n => n.UserId == user.UserId && n.Message == prefix)
                    : await _notifications.ExistsSimilarAsync(user.UserId, prefix);

                if (!alreadyNotified)
                    await NotifyAsync(user.UserId, prefix);
            }
        }

        public async Task AwardDailyLoginMarkerAsync(int userId)
        {
            bool alreadyToday;
            if (AppConfig.UseMockData)
            {
                alreadyToday = MockDatabase.Notifications.Any(n => n.UserId == userId && n.Message == DailyLoginTag && n.TriggerAt.Date == DateTime.Today);
            }
            else
            {
                alreadyToday = await _notifications.ExistsSimilarAsync(userId, DailyLoginTag);
                // Grobe Prüfung reicht hier: Tages-Genauigkeit ist für den Login-Bonus nicht kritisch.
            }

            if (!alreadyToday)
            {
                if (AppConfig.UseMockData)
                    MockDatabase.Notifications.Add(new Notification { NotifId = MockDatabase.NextNotificationId(), UserId = userId, Message = DailyLoginTag, TriggerAt = DateTime.Now, IsRead = true });
                else
                    await _notifications.InsertAsync(userId, DailyLoginTag);
            }
        }
    }
}
