using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Data;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    /// <summary>Lernplaner: Lernzeiten erfassen (FA-26).</summary>
    public class StudyService
    {
        private readonly StudyRepository _study = new();

        public async Task<List<StudySession>> GetSessionsForUserAsync(int userId)
        {
            if (AppConfig.UseMockData)
                return MockDatabase.StudySessions.Where(s => s.UserId == userId).OrderByDescending(s => s.StartTime).ToList();

            return await _study.GetForUserAsync(userId);
        }

        public async Task<int> GetTotalMinutesAsync(int userId)
        {
            if (AppConfig.UseMockData)
                return MockDatabase.StudySessions.Where(s => s.UserId == userId).Sum(s => s.DurationMin);

            return await _study.GetTotalMinutesAsync(userId);
        }

        public async Task<StudySession> LogSessionAsync(int userId, int taskId, int durationMin, string notes)
        {
            var session = new StudySession
            {
                UserId = userId,
                TaskId = (uint)taskId,
                StartTime = System.DateTime.Now,
                DurationMin = durationMin,
                Notes = notes ?? string.Empty
            };

            if (AppConfig.UseMockData)
            {
                session.SessionId = MockDatabase.NextStudySessionId();
                MockDatabase.StudySessions.Add(session);
            }
            else
            {
                await _study.LogSessionAsync(userId, taskId, durationMin, notes ?? string.Empty);
            }

            await new GamificationService().AwardOnStudySessionAsync(userId, durationMin);
            return session;
        }
    }
}
