using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Data;
using App_LernPlaner.Helpers;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    /// <summary>Punkte, Badges und Rangliste (FA-30 bis FA-32).</summary>
    public class GamificationService
    {
        private readonly GamificationRepository _gamification = new();
        private readonly UserRepository _users = new();

        public async Task AddXpAsync(int userId, int amount)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current != null) return;
            if (AppConfig.UseMockData)
            {
                var user = MockDatabase.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null) user.XPPoints += amount;
                return;
            }

            await _users.AddXpAsync(userId, amount);
        }

        public async Task AwardOnSubmissionAsync(int userId, bool isLate)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current != null) return;
            await AddXpAsync(userId, isLate ? XpHelpers.LateSubmission : XpHelpers.OnTimeSubmission);

            var submissionService = new SubmissionService();
            var mySubmissions = await submissionService.GetSubmissionsForUserAsync(userId);

            if (mySubmissions.Count == 1)
                await AwardBadgeAsync(userId, "Erste Abgabe");

            var taskRepo = new TaskRepository();
            int onTimeCount = 0;
            foreach (var s in mySubmissions)
            {
                var task = AppConfig.UseMockData
                    ? MockDatabase.Tasks.FirstOrDefault(t => t.TaskId == s.TaskId)
                    : await taskRepo.GetByIdAsync(s.TaskId);
                if (task != null && s.SubmittedAt <= task.Deadline) onTimeCount++;
            }

            if (onTimeCount >= 5)
                await AwardBadgeAsync(userId, "Fleißiger Schüler");
        }

        public async Task AwardOnGradeAsync(int userId, int gradeValue)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current != null) return;
            if (gradeValue == 1)
            {
                await AddXpAsync(userId, XpHelpers.Grade1);
                await AwardBadgeAsync(userId, "Streber");
            }
            else if (gradeValue == 2)
            {
                await AddXpAsync(userId, XpHelpers.Grade2);
            }
        }

        public async Task AwardOnStudySessionAsync(int userId, int durationMin)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current != null) return;
            int hours = durationMin / 60;
            if (hours > 0) await AddXpAsync(userId, hours * XpHelpers.StudyHour);

            int totalMinutes = await new StudyService().GetTotalMinutesAsync(userId);
            if (totalMinutes >= 600)
                await AwardBadgeAsync(userId, "Lernmarathon");
        }

        public async Task AwardDailyLoginAsync(int userId)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current != null) return;
            await new NotificationService().AwardDailyLoginMarkerAsync(userId);
            await AddXpAsync(userId, XpHelpers.DailyLogin);
        }

        public async Task AwardSubTaskCheckedAsync(int userId) => await AddXpAsync(userId, XpHelpers.SubTaskChecked);

        private async Task AwardBadgeAsync(int userId, string badgeName)
        {
            var allBadges = AppConfig.UseMockData ? MockDatabase.Badges : await _gamification.GetAllBadgesAsync();
            var badge = allBadges.FirstOrDefault(b => b.Name == badgeName);
            if (badge == null) return;

            bool alreadyEarned = AppConfig.UseMockData
                ? MockDatabase.UserBadges.Any(ub => ub.UserId == userId && ub.BadgeId == badge.BadgeId)
                : await _gamification.HasBadgeAsync(userId, badge.BadgeId);

            if (alreadyEarned) return;

            if (AppConfig.UseMockData)
                MockDatabase.UserBadges.Add(new UserBadge { UserId = userId, BadgeId = badge.BadgeId, DateEarned = DateTime.Now, Badge = badge });
            else
                await _gamification.AwardBadgeAsync(userId, badge.BadgeId);

            await AddXpAsync(userId, badge.XPReward);
            await new NotificationService().NotifyAsync(userId, $"Neues Badge erhalten: {badge.Name} ({badge.IconPath})");
        }

        public async Task<List<UserBadge>> GetBadgesAsync(int userId)
        {
            if (AppConfig.UseMockData)
                return MockDatabase.UserBadges.Where(ub => ub.UserId == userId).OrderByDescending(ub => ub.DateEarned).ToList();

            return await _gamification.GetUserBadgesAsync(userId);
        }

        // FA-32: Rangliste innerhalb einer Klasse
        public async Task<List<User>> GetLeaderboardAsync(int classId)
        {
            if (AppConfig.UseMockData)
                return (await new ClassService().GetMembersAsync(classId, role: 2)).OrderByDescending(u => u.XPPoints).ToList();

            return await _gamification.GetLeaderboardAsync(classId);
        }
    }
}
