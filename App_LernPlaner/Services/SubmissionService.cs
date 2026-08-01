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
            var submission = new Submission
            {
                TaskId = taskId,
                UserId = userId,
                Status = status,
                Notizen = notizen,
                Abgabe = abgabe,
                LearningMinutes = learningMinutes,
                FilePath = filePath,
                SubmittedAt = DateTime.UtcNow
            };

            await _submissionRepository.SaveProgressAsync(submission);
        }
    }
}
