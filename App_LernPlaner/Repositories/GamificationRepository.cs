using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Repositories
{
    public class GamificationRepository
    {
        private readonly db_verbindung _db = new();

        public async Task<List<Badge>> GetAllBadgesAsync()
        {
            var result = new List<Badge>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Badges", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(MapBadge(reader));

            return result;
        }

        public async Task<List<UserBadge>> GetUserBadgesAsync(int userId)
        {
            var result = new List<UserBadge>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = @"SELECT ub.UserId, ub.BadgeId, ub.EarnedAt, b.Name, b.Description, b.IconPath, b.XPReward
                                    FROM UserBadge ub
                                    INNER JOIN Badges b ON b.BadgeId = ub.BadgeId
                                    WHERE ub.UserId = @u
                                    ORDER BY ub.EarnedAt DESC";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new UserBadge
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    BadgeId = Convert.ToInt32(reader["BadgeId"]),
                    DateEarned = reader["EarnedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["EarnedAt"]),
                    Badge = new Badge
                    {
                        BadgeId = Convert.ToInt32(reader["BadgeId"]),
                        Name = reader["Name"] as string ?? string.Empty,
                        Description = reader["Description"] as string ?? string.Empty,
                        IconPath = reader["IconPath"] as string ?? string.Empty,
                        XPReward = reader["XPReward"] == DBNull.Value ? 0 : Convert.ToInt32(reader["XPReward"])
                    }
                });
            }

            return result;
        }

        public async Task<bool> HasBadgeAsync(int userId, int badgeId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM UserBadge WHERE UserId=@u AND BadgeId=@b", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@b", badgeId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        public async Task<int?> GetBadgeIdByNameAsync(string name)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT BadgeId FROM Badges WHERE Name = @n LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@n", name);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        public async Task AwardBadgeAsync(int userId, int badgeId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "INSERT INTO UserBadge (UserId, BadgeId, EarnedAt) VALUES (@u, @b, @d)", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@b", badgeId);
            cmd.Parameters.AddWithValue("@d", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        // FA-32: Rangliste innerhalb einer Klasse
        public async Task<List<User>> GetLeaderboardAsync(int classId)
        {
            var result = new List<User>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = @"SELECT u.* FROM Users u
                                    INNER JOIN ClassMembers cm ON cm.UserId = u.UserId
                                    WHERE cm.ClassId = @c AND cm.Role = 2
                                    ORDER BY u.XPPoints DESC";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@c", classId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new User
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    UserName = reader["Username"] as string ?? string.Empty,
                    XPPoints = reader["XPPoints"] == DBNull.Value ? 0 : Convert.ToInt32(reader["XPPoints"]),
                    FirstName = reader["FirstName"] as string,
                    LastName = reader["LastName"] as string
                });
            }

            return result;
        }

        private static Badge MapBadge(IDataRecord r) => new()
        {
            BadgeId = Convert.ToInt32(r["BadgeId"]),
            Name = r["Name"] as string ?? string.Empty,
            Description = r["Description"] as string ?? string.Empty,
            IconPath = r["IconPath"] as string ?? string.Empty,
            XPReward = r["XPReward"] == DBNull.Value ? 0 : Convert.ToInt32(r["XPReward"])
        };
    }
}
