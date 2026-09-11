using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Repositories
{
    public class StudyRepository
    {
        private readonly db_verbindung _db = new();

        // FA-26: Schüler erfasst Lernzeiten
        public async Task LogSessionAsync(int userId, int taskId, int durationMin, string notes)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "INSERT INTO StudySessions (UserId, TaskId, StartTime, DurationMin, Notes) VALUES (@u, @t, @s, @d, @n)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@t", taskId == 0 ? (object)DBNull.Value : taskId);
            cmd.Parameters.AddWithValue("@s", DateTime.Now);
            cmd.Parameters.AddWithValue("@d", durationMin);
            cmd.Parameters.AddWithValue("@n", notes);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<StudySession>> GetForUserAsync(int userId)
        {
            var result = new List<StudySession>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM StudySessions WHERE UserId = @u ORDER BY StartTime DESC", conn);
            cmd.Parameters.AddWithValue("@u", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new StudySession
                {
                    SessionId = Convert.ToInt32(reader["SessionId"]),
                    UserId = Convert.ToInt32(reader["UserId"]),
                    TaskId = reader["TaskId"] == DBNull.Value ? 0 : Convert.ToUInt32(reader["TaskId"]),
                    StartTime = reader["StartTime"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["StartTime"]),
                    DurationMin = reader["DurationMin"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DurationMin"]),
                    Notes = reader["Notes"] as string ?? string.Empty
                });
            }

            return result;
        }

        public async Task<int> GetTotalMinutesAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COALESCE(SUM(DurationMin),0) FROM StudySessions WHERE UserId = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }
}
