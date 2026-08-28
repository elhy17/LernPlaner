using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<Submission?> GetByTaskAndUserAsync(int taskId, int userId)
        {
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = "SELECT * FROM Submissions WHERE TaskId = @taskId AND UserId = @userId LIMIT 1";
            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@taskId", taskId);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<List<Submission>> GetForTaskAsync(int taskId)
        {
            var result = new List<Submission>();
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            using MySqlCommand cmd = new("SELECT * FROM Submissions WHERE TaskId = @taskId ORDER BY SubmittedAt DESC", conn);
            cmd.Parameters.AddWithValue("@taskId", taskId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        public async Task<List<Submission>> GetForUserAsync(int userId)
        {
            var result = new List<Submission>();
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            using MySqlCommand cmd = new("SELECT * FROM Submissions WHERE UserId = @userId ORDER BY SubmittedAt DESC", conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        // FA-20/21: Note (1-6) und Kommentar vergeben
        public async Task GradeAsync(int submissionId, string grade, string feedback)
        {
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            using MySqlCommand cmd = new("UPDATE Submissions SET Grade=@g, Feedback=@f WHERE SubmissionId=@id", conn);
            cmd.Parameters.AddWithValue("@g", grade);
            cmd.Parameters.AddWithValue("@f", feedback);
            cmd.Parameters.AddWithValue("@id", submissionId);
            await cmd.ExecuteNonQueryAsync();
        }

        private static Submission Map(IDataRecord r) => new()
        {
            SubmissionId = Convert.ToInt32(r["SubmissionId"]),
            TaskId = r["TaskId"] == DBNull.Value ? 0 : Convert.ToInt32(r["TaskId"]),
            UserId = r["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(r["UserId"]),
            Status = r["Status"] == DBNull.Value ? (byte)0 : Convert.ToByte(r["Status"]),
            Notizen = r["Notizen"] as string ?? string.Empty,
            Abgabe = r["Abgabe"] as string ?? string.Empty,
            LearningMinutes = r["LearningMinutes"] == DBNull.Value ? 0 : Convert.ToInt32(r["LearningMinutes"]),
            FilePath = r["FilePath"] as string ?? string.Empty,
            SubmittedAt = r["SubmittedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["SubmittedAt"]),
            Grade = r["Grade"] as string,
            Feedback = r["Feedback"] as string
        };
    }
}
