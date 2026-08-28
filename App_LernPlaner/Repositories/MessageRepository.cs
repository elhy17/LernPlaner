using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Repositories
{
    /// <summary>
    /// Chat zwischen beliebigen Benutzern (Schüler/Lehrer/Admin), rein DB-basiert
    /// (kein WebSocket - wird per Timer/Polling im UI aktualisiert).
    /// </summary>
    public class MessageRepository
    {
        private readonly db_verbindung _db = new();

        public async Task<List<Message>> GetConversationAsync(int userA, int userB)
        {
            var result = new List<Message>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = @"SELECT m.*, u.Username AS SenderName FROM Messages m
                                    INNER JOIN Users u ON u.UserId = m.SenderId
                                    WHERE (m.SenderId = @a AND m.ReceiverId = @b)
                                       OR (m.SenderId = @b AND m.ReceiverId = @a)
                                    ORDER BY m.SentAt ASC";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@a", userA);
            cmd.Parameters.AddWithValue("@b", userB);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        // Für die Kontaktliste: jeder andere Benutzer im System + letzte Nachricht + ungelesen-Anzahl
        public async Task<List<Contact>> GetContactsWithLastMessageAsync(int userId)
        {
            var result = new List<Contact>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string usersQuery = "SELECT UserId, Username, Role, FirstName, LastName FROM Users WHERE UserId != @me AND IsActive = 1 ORDER BY Username";
            using (var cmd = new MySqlCommand(usersQuery, conn))
            {
                cmd.Parameters.AddWithValue("@me", userId);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string first = reader["FirstName"] as string ?? string.Empty;
                    string last = reader["LastName"] as string ?? string.Empty;
                    string name = string.IsNullOrWhiteSpace(first + last) ? (reader["Username"] as string ?? "") : $"{first} {last}".Trim();

                    result.Add(new Contact
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        DisplayName = name,
                        Role = reader["Role"] as string ?? string.Empty
                    });
                }
            }

            foreach (var contact in result)
            {
                using var cmd = new MySqlCommand(
                    @"SELECT Body, SentAt FROM Messages
                      WHERE (SenderId = @me AND ReceiverId = @other) OR (SenderId = @other AND ReceiverId = @me)
                      ORDER BY SentAt DESC LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@me", userId);
                cmd.Parameters.AddWithValue("@other", contact.UserId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        contact.LastMessage = reader["Body"] as string ?? string.Empty;
                        contact.LastMessageAt = reader["SentAt"] == DBNull.Value ? null : Convert.ToDateTime(reader["SentAt"]);
                    }
                }

                using var unreadCmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM Messages WHERE SenderId = @other AND ReceiverId = @me AND IsRead = 0 AND IsDeleted = 0", conn);
                unreadCmd.Parameters.AddWithValue("@other", contact.UserId);
                unreadCmd.Parameters.AddWithValue("@me", userId);
                contact.UnreadCount = Convert.ToInt32(await unreadCmd.ExecuteScalarAsync());
            }

            result.Sort((a, b) => Nullable.Compare(b.LastMessageAt, a.LastMessageAt));
            return result;
        }

        public async Task<int> CountUnreadTotalAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Messages WHERE ReceiverId = @u AND IsRead = 0 AND IsDeleted = 0", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<int> SendAsync(int senderId, int receiverId, string body)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "INSERT INTO Messages (SenderId, ReceiverId, Body, SentAt, IsDeleted, IsRead) VALUES (@s, @r, @b, @t, 0, 0)", conn);
            cmd.Parameters.AddWithValue("@s", senderId);
            cmd.Parameters.AddWithValue("@r", receiverId);
            cmd.Parameters.AddWithValue("@b", body);
            cmd.Parameters.AddWithValue("@t", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        // Nur eigene Nachrichten dürfen bearbeitet werden - Aufrufer prüft senderId == aktueller Benutzer.
        public async Task EditAsync(int messageId, int senderId, string newBody)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE Messages SET Body = @b, EditedAt = @e WHERE MessageId = @id AND SenderId = @s", conn);
            cmd.Parameters.AddWithValue("@b", newBody);
            cmd.Parameters.AddWithValue("@e", DateTime.Now);
            cmd.Parameters.AddWithValue("@id", messageId);
            cmd.Parameters.AddWithValue("@s", senderId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Nur eigene Nachrichten dürfen gelöscht werden - Aufrufer prüft senderId == aktueller Benutzer.
        public async Task SoftDeleteAsync(int messageId, int senderId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE Messages SET IsDeleted = 1 WHERE MessageId = @id AND SenderId = @s", conn);
            cmd.Parameters.AddWithValue("@id", messageId);
            cmd.Parameters.AddWithValue("@s", senderId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task MarkConversationReadAsync(int userId, int otherUserId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "UPDATE Messages SET IsRead = 1 WHERE ReceiverId = @me AND SenderId = @other AND IsRead = 0", conn);
            cmd.Parameters.AddWithValue("@me", userId);
            cmd.Parameters.AddWithValue("@other", otherUserId);
            await cmd.ExecuteNonQueryAsync();
        }

        // Für den Admin: seine gesamte Inbox als "Anfragen" (siehe AdminDashboard).
        public async Task<List<Message>> GetInboxAsync(int userId)
        {
            var result = new List<Message>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = @"SELECT m.*, u.Username AS SenderName FROM Messages m
                                    INNER JOIN Users u ON u.UserId = m.SenderId
                                    WHERE m.ReceiverId = @u AND m.IsDeleted = 0
                                    ORDER BY m.SentAt DESC";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        private static Message Map(IDataRecord r) => new()
        {
            MessageId = Convert.ToInt32(r["MessageId"]),
            SenderId = Convert.ToInt32(r["SenderId"]),
            ReceiverId = Convert.ToInt32(r["ReceiverId"]),
            Body = r["IsDeleted"] != DBNull.Value && Convert.ToBoolean(r["IsDeleted"]) ? "(Nachricht gelöscht)" : (r["Body"] as string ?? string.Empty),
            SentAt = r["SentAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["SentAt"]),
            EditedAt = r["EditedAt"] == DBNull.Value ? null : Convert.ToDateTime(r["EditedAt"]),
            IsDeleted = r["IsDeleted"] != DBNull.Value && Convert.ToBoolean(r["IsDeleted"]),
            IsRead = r["IsRead"] != DBNull.Value && Convert.ToBoolean(r["IsRead"]),
            SenderName = r["SenderName"] as string ?? string.Empty
        };
    }
}
