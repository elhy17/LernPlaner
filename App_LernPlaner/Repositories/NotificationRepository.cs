using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Repositories
{
    public class NotificationRepository
    {
        private readonly db_verbindung _db = new();

        public async Task<List<Notification>> GetForUserAsync(int userId)
        {
            var result = new List<Notification>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Notifications WHERE UserId = @u ORDER BY TriggerAt DESC", conn);
            cmd.Parameters.AddWithValue("@u", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        public async Task<int> CountUnreadAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserId = @u AND (IsRead = 0 OR IsRead IS NULL)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task InsertAsync(int userId, string message)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "INSERT INTO Notifications (UserId, Message, TriggerAt, IsRead) VALUES (@u, @m, @t, 0)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", message);
            cmd.Parameters.AddWithValue("@t", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> ExistsSimilarAsync(int userId, string messagePrefix)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Notifications WHERE UserId = @u AND Message LIKE @m", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@m", messagePrefix + "%");
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        public async Task MarkAsReadAsync(int notifId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Notifications SET IsRead = 1 WHERE NotifId = @id", conn);
            cmd.Parameters.AddWithValue("@id", notifId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Notifications SET IsRead = 1 WHERE UserId = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        private static Notification Map(IDataRecord r) => new()
        {
            NotifId = Convert.ToInt32(r["NotifId"]),
            UserId = r["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(r["UserId"]),
            Message = r["Message"] as string ?? string.Empty,
            TriggerAt = r["TriggerAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["TriggerAt"]),
            IsRead = r["IsRead"] != DBNull.Value && Convert.ToBoolean(r["IsRead"])
        };
    }
}
