using App_LernPlaner.Models;
using App_LernPlaner.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Services
{
   public class SubmissionService
    {
        private readonly SubmissionRepository _submissionRepository;

        public SubmissionService()
        {
            _submissionRepository = new SubmissionRepository();
        }

        public async Task SaveProgressAsync(int taskId, int userId, byte status, string notizen, string abgabe, int learningMinutes, string filePath)
        {
            var submittedAt = DateTime.Now;
            var submission = new Submission
            {
                TaskId = taskId,
                UserId = userId,
                Status = status,
                Notizen = notizen,
                Abgabe = abgabe,
                LearningMinutes = learningMinutes,
                FilePath = filePath,
                SubmittedAt = submittedAt
            };

            await _submissionRepository.SaveProgressAsync(submission);

            // FA-30: XP für pünktliche/verspätete Abgabe, FA-26: XP für erfasste Lernzeit
            if (status == 2)
            {
                var task = await new TaskService().GetByIdAsync(taskId);
                bool isLate = task != null && submittedAt > task.Deadline;
                var gamification = new GamificationService();
                await gamification.AwardOnSubmissionAsync(userId, isLate);
                if (learningMinutes > 0)
                    await gamification.AwardOnStudySessionAsync(userId, learningMinutes);

                if (task?.CreatedBy is int creatorId && creatorId != userId)
                    await new NotificationService().NotifyAsync(creatorId, $"Neue Abgabe für: {task.Titel}");
            }
        }

        public async Task<List<Submission>> GetSubmissionsForUserAsync(int userId)
        {
            return await _submissionRepository.GetForUserAsync(userId);
        }

        public async Task<List<Submission>> GetSubmissionsForTaskAsync(int taskId)
        {
            return await _submissionRepository.GetForTaskAsync(taskId);
        }

        // FA-20/21, UC-11: Lehrer vergibt Note (1-6) und Kommentar
        public async Task<(bool Success, string Error)> GradeAsync(int submissionId, int taskId, int studentUserId, string grade, string feedback)
        {
            if (!int.TryParse(grade, out int gradeValue) || gradeValue < 1 || gradeValue > 6)
                return (false, "Note muss zwischen 1 und 6 liegen.");

            await _submissionRepository.GradeAsync(submissionId, grade, feedback ?? string.Empty);

            await new GamificationService().AwardOnGradeAsync(studentUserId, gradeValue);

            var task = await new TaskService().GetByIdAsync(taskId);
            string title = task?.Titel ?? "einer Aufgabe";
            await new NotificationService().NotifyAsync(studentUserId, $"Du hast eine Bewertung erhalten: {title} (Note {grade})");

            return (true, string.Empty);
        }
    }
}
