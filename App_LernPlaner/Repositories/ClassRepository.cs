using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Repositories
{
    public class ClassRepository
    {
        private readonly db_verbindung _db = new();

        public async Task<List<Class>> GetAllAsync()
        {
            var result = new List<Class>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Classes ORDER BY Name", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        public async Task<Class?> GetByIdAsync(int classId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Classes WHERE ClassId = @id", conn);
            cmd.Parameters.AddWithValue("@id", classId);

            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<Class?> GetByJoinCodeAsync(string joinCode)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Classes WHERE JoinCode = @code", conn);
            cmd.Parameters.AddWithValue("@code", joinCode);

            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        // FA-14 Grundlage: Klassen, in denen der Benutzer Mitglied ist (Schüler oder Lehrer)
        public async Task<List<Class>> GetForUserAsync(int userId)
        {
            var result = new List<Class>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = @"SELECT c.* FROM Classes c
                                    INNER JOIN ClassMembers cm ON cm.ClassId = c.ClassId
                                    WHERE cm.UserId = @userId
                                    ORDER BY c.Name";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        public async Task<List<User>> GetMembersAsync(int classId, byte? role = null)
        {
            var result = new List<User>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"SELECT u.* FROM Users u
                              INNER JOIN ClassMembers cm ON cm.UserId = u.UserId
                              WHERE cm.ClassId = @classId" + (role.HasValue ? " AND cm.Role = @role" : "");
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@classId", classId);
            if (role.HasValue) cmd.Parameters.AddWithValue("@role", role.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(UserRepositoryMap(reader));

            return result;
        }

        public async Task<int> CountStudentsAsync(int classId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM ClassMembers WHERE ClassId = @id AND Role = 2", conn);
            cmd.Parameters.AddWithValue("@id", classId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        // FA-07/FA-08: Klasse erstellen mit eindeutigem Einladungscode
        public async Task<Class> CreateAsync(string name, int teacherId, string joinCode)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var createdAt = DateTime.Now;

            using (var cmd = new MySqlCommand(
                "INSERT INTO Classes (Name, TeacherId, JoinCode, CreatedAt) VALUES (@n, @t, @j, @c)", conn))
            {
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@t", teacherId);
                cmd.Parameters.AddWithValue("@j", joinCode);
                cmd.Parameters.AddWithValue("@c", createdAt);
                await cmd.ExecuteNonQueryAsync();

                int newId = (int)cmd.LastInsertedId;

                using var memberCmd = new MySqlCommand(
                    "INSERT INTO ClassMembers (ClassId, UserId, Role, JoinedAt) VALUES (@c, @u, 1, @j)", conn);
                memberCmd.Parameters.AddWithValue("@c", newId);
                memberCmd.Parameters.AddWithValue("@u", teacherId);
                memberCmd.Parameters.AddWithValue("@j", createdAt);
                await memberCmd.ExecuteNonQueryAsync();

                return new Class { ClassId = newId, Name = name, TeacherId = teacherId, JoinCode = joinCode, CreatedAt = createdAt };
            }
        }

        public async Task<bool> IsMemberAsync(int classId, int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM ClassMembers WHERE ClassId = @c AND UserId = @u", conn);
            cmd.Parameters.AddWithValue("@c", classId);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        // FA-09: Schüler tritt per Code bei
        public async Task AddMemberAsync(int classId, int userId, byte role)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand(
                "INSERT INTO ClassMembers (ClassId, UserId, Role, JoinedAt) VALUES (@c, @u, @r, @j)", conn);
            cmd.Parameters.AddWithValue("@c", classId);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@j", DateTime.Now);
            await cmd.ExecuteNonQueryAsync();
        }

        // FA-10: Lehrer entfernt Schüler aus Klasse
        public async Task RemoveMemberAsync(int classId, int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("DELETE FROM ClassMembers WHERE ClassId = @c AND UserId = @u", conn);
            cmd.Parameters.AddWithValue("@c", classId);
            cmd.Parameters.AddWithValue("@u", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        // FA-11: Klasse archivieren/löschen
        public async Task DeleteAsync(int classId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using (var cmd = new MySqlCommand("DELETE FROM ClassMembers WHERE ClassId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", classId);
                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new MySqlCommand("UPDATE Tasks SET ClassId = NULL WHERE ClassId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", classId);
                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new MySqlCommand("DELETE FROM Classes WHERE ClassId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", classId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static Class Map(IDataRecord r) => new()
        {
            ClassId = Convert.ToInt32(r["ClassId"]),
            Name = r["Name"] as string ?? string.Empty,
            TeacherId = r["TeacherId"] == DBNull.Value ? 0 : Convert.ToInt32(r["TeacherId"]),
            JoinCode = r["JoinCode"] as string ?? string.Empty,
            CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["CreatedAt"])
        };

        private static User UserRepositoryMap(IDataRecord r) => new()
        {
            UserId = Convert.ToInt32(r["UserId"]),
            UserName = r["Username"] as string ?? string.Empty,
            Email = r["Email"] as string ?? string.Empty,
            Role = r["Role"] as string ?? Roles.Schueler,
            XPPoints = r["XPPoints"] == DBNull.Value ? 0 : Convert.ToInt32(r["XPPoints"]),
            FirstName = r["FirstName"] as string,
            LastName = r["LastName"] as string
        };
    }
}
