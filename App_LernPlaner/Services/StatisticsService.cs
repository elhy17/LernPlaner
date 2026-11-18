using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App_LernPlaner.Data;
using App_LernPlaner.Models;
using App_LernPlaner.Repositories;

namespace App_LernPlaner.Services
{
    public class ClassStatistics
    {
        public string ClassName { get; set; } = string.Empty;
        public int StudentCount { get; set; }
        public int TaskCount { get; set; }
        public int SubmissionCount { get; set; }
        public double SubmissionRate { get; set; } // FA-28: Abgabequote
    }

    public class SystemStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalClasses { get; set; }
        public int TotalTasks { get; set; }
        public int TotalSubmissions { get; set; }
    }

    /// <summary>Lernfortschritt und Statistiken (FA-27 bis FA-29) - lädt bei jedem Aufruf
    /// live aus der Datenbank, damit die Zahlen immer aktuell sind.</summary>
    public class StatisticsService
    {
        private readonly TaskService _taskService = new();
        private readonly SubmissionService _submissionService = new();
        private readonly ClassService _classService = new();
        private readonly UserRepository _users = new();

        // FA-27: Dashboard zeigt erledigte Aufgaben, Lernzeit, Durchschnittsnote
        public async Task<DashboardStets> GetStudentStatsAsync(User user)
        {
            var visibleTasks = await _taskService.GetVisibleTasksForUserAsync(user);
            var mySubmissions = await _submissionService.GetSubmissionsForUserAsync(user.UserId);
            var gradedSubmissions = mySubmissions.Where(s => s.IsGraded).ToList();

            double avgGrade = gradedSubmissions.Count > 0 ? gradedSubmissions.Average(s => double.Parse(s.Grade!)) : 0;

            int openCount = 0;
            foreach (var t in visibleTasks)
                if (await _taskService.GetStatusForStudentAsync(t, user.UserId) == 0) openCount++;

            int currentXp = AppConfig.UseMockData ? user.XPPoints : (await _users.GetByIdAsync(user.UserId))?.XPPoints ?? user.XPPoints;

            return new DashboardStets
            {
                OffeneAufgaben = openCount,
                Abgegeben = mySubmissions.Count,
                Durchschnittsnote = gradedSubmissions.Count > 0 ? avgGrade.ToString("0.0") : "-",
                XpGesamt = currentXp
            };
        }

        public async Task<int> GetTotalStudyMinutesAsync(int userId) => await new StudyService().GetTotalMinutesAsync(userId);

        public async Task<int> CountOnTimeSubmissionsAsync(int userId)
        {
            var submissions = await _submissionService.GetSubmissionsForUserAsync(userId);
            var taskRepo = new TaskRepository();
            int count = 0;
            foreach (var s in submissions)
            {
                var task = AppConfig.UseMockData
                    ? MockDatabase.Tasks.FirstOrDefault(t => t.TaskId == s.TaskId)
                    : await taskRepo.GetByIdAsync(s.TaskId);
                if (task != null && s.SubmittedAt <= task.Deadline) count++;
            }
            return count;
        }

        // FA-28: Lehrer sehen Klassenstatistiken (Abgabequote)
        public async Task<ClassStatistics> GetClassStatisticsAsync(Class cls)
        {
            if (!AppConfig.UseMockData && Remote.RemoteClient.Current is { } remote)
                return await remote.PostAsync<ClassStatistics>("classes/statistics",cls.ClassId);
            var students = await _classService.GetMembersAsync(cls.ClassId, role: 2);

            var allTasks = (await _taskService.GetTasksByClassIdAsync(cls.ClassId));

            int expectedSubmissions = allTasks.Count * Math.Max(students.Count, 1);

            int actualSubmissions = 0;
            foreach (var task in allTasks)
                actualSubmissions += (await _submissionService.GetSubmissionsForTaskAsync(task.TaskId)).Count;

            return new ClassStatistics
            {
                ClassName = cls.Name,
                StudentCount = students.Count,
                TaskCount = allTasks.Count,
                SubmissionCount = actualSubmissions,
                SubmissionRate = expectedSubmissions == 0 ? 0 : Math.Round(100.0 * actualSubmissions / expectedSubmissions, 1)
            };
        }

        // FA-29: Admin sieht systemweite Statistiken
        public async Task<SystemStatistics> GetSystemStatisticsAsync()
        {
            var users = AppConfig.UseMockData ? MockDatabase.Users : await _users.GetAllAsync();
            var classes = AppConfig.UseMockData ? MockDatabase.Classes : await _classService.GetAllClassesAsync();
            var tasks = await _taskService.GetTasksAsync();

            int totalSubmissions = 0;
            foreach (var t in tasks)
                totalSubmissions += (await _submissionService.GetSubmissionsForTaskAsync(t.TaskId)).Count;

            return new SystemStatistics
            {
                TotalUsers = users.Count,
                TotalStudents = users.Count(u => u.IsStudent),
                TotalTeachers = users.Count(u => u.IsTeacher),
                TotalClasses = classes.Count,
                TotalTasks = tasks.Count,
                TotalSubmissions = totalSubmissions
            };
        }
    }
}
