using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Repositories
{
    public class SubmissionRepository
    {
        private readonly db_verbindung _db;

        public SubmissionRepository()
        {
            _db = new db_verbindung();
        }

        public async Task SaveProgressAsync(Submission submission)
        {
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"INSERT INTO Submissions(TaskId, UserId, Status, Notizen, Abgabe, LearningMinutes, SubmittedAt, FilePath) 
               VALUES (@taskId,@userId,@status,@notizen,@abgabe,@learningMinutes,@submittedAt,@filepath)
               ON DUPLICATE KEY UPDATE
                Status = @status,
                Notizen = @notizen,
                Abgabe = @abgabe,
                LearningMinutes = @learningMinutes,
                FilePath = @filePath,
                SubmittedAt = @submittedAt;";

            using MySqlCommand cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@taskId", submission.TaskId);
            cmd.Parameters.AddWithValue("@userId", submission.UserId);
            cmd.Parameters.AddWithValue("@status", submission.Status);
            cmd.Parameters.AddWithValue("@notizen", submission.Notizen);
            cmd.Parameters.AddWithValue("@abgabe", submission.Abgabe);
            cmd.Parameters.AddWithValue("@learningMinutes", submission.LearningMinutes);
            cmd.Parameters.AddWithValue("@submittedAt", submission.SubmittedAt);
            cmd.Parameters.AddWithValue("@filepath", submission.FilePath);

            await cmd.ExecuteNonQueryAsync();


        }
    }
}
