using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using App_LernPlaner.Database;
using App_LernPlaner.Models;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Repositories
{
    public class UserRepository
    {
        private readonly db_verbindung _db = new();

        public async Task<User?> FindByUsernameAsync(string username)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = "SELECT * FROM Users WHERE Username = @u LIMIT 1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);

            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = "SELECT * FROM Users WHERE Email = @e LIMIT 1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@e", email);

            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = "SELECT * FROM Users WHERE UserId = @id LIMIT 1";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var result = new List<User>();
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT * FROM Users ORDER BY Username", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                result.Add(Map(reader));

            return result;
        }

        public async Task<bool> ExistsAsync(string username, string email)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = "SELECT COUNT(*) FROM Users WHERE Username = @u OR Email = @e";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@e", email);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        public async Task<int> InsertAsync(string username, string passwordHash, string email, string role)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string query = @"INSERT INTO Users (Username, PasswordHash, Email, Role, ProfilePicturePath, XPPoints, CreatedAt, IsActive)
                                    VALUES (@u, @p, @e, @r, 'default.png', 0, @created, 1)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@created", DateTime.Now);

            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        public async Task UpdatePasswordAsync(int userId, string newHash)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Users SET PasswordHash = @p WHERE UserId = @id", conn);
            cmd.Parameters.AddWithValue("@p", newHash);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateRoleAsync(int userId, string role)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Users SET Role = @r WHERE UserId = @id", conn);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateActiveAsync(int userId, bool isActive)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Users SET IsActive = @a WHERE UserId = @id", conn);
            cmd.Parameters.AddWithValue("@a", isActive);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        // FA-06: Benutzerprofil bearbeiten (Name, Avatar)
        public async Task UpdateProfileAsync(int userId, string firstName, string lastName, string? profilePicturePath = null)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = "UPDATE Users SET FirstName = @f, LastName = @l" +
                            (profilePicturePath != null ? ", ProfilePicturePath = @p" : "") +
                            " WHERE UserId = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@f", firstName);
            cmd.Parameters.AddWithValue("@l", lastName);
            if (profilePicturePath != null) cmd.Parameters.AddWithValue("@p", profilePicturePath);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddXpAsync(int userId, int amount)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("UPDATE Users SET XPPoints = XPPoints + @amt WHERE UserId = @id", conn);
            cmd.Parameters.AddWithValue("@amt", amount);
            cmd.Parameters.AddWithValue("@id", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        // FA-06: Profilbearbeitung über das UserProfile-DTO (ProfileViewModel)
        public async Task UpdateUserProfileAsync(UserProfile profile)
        {
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = "UPDATE Users SET FirstName=@firstName , LastName=@lastName, Email=@email WHERE UserId = @userId; ";

            using MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", profile.UserId);
            cmd.Parameters.AddWithValue("@firstName", profile.FirstName);
            cmd.Parameters.AddWithValue("@lastName", profile.LastName);
            cmd.Parameters.AddWithValue("@email", profile.Email);

            await cmd.ExecuteNonQueryAsync();
        }

        // Behalten für Repositories/ViewModels, die weiterhin das ältere UserProfile-DTO erwarten.
        public async Task<UserProfile?> GetUserProfileByIdAsync(int userId)
        {
            var u = await GetByIdAsync(userId);
            if (u == null) return null;

            return new UserProfile
            {
                UserId = u.UserId,
                Username = u.UserName,
                Email = u.Email,
                Role = u.Role,
                ProfilePicturePath = u.ProfilePicture,
                XPPoints = u.XPPoints,
                CreatedAt = u.CreatedAt,
                FirstName = u.FirstName ?? string.Empty,
                LastName = u.LastName ?? string.Empty,
                ClassName = u.ClassName ?? string.Empty
            };
        }

        private static User Map(IDataRecord r) => new()
        {
            UserId = Convert.ToInt32(r["UserId"]),
            UserName = r["Username"] as string ?? string.Empty,
            PasswordHash = r["PasswordHash"] as string ?? string.Empty,
            Email = r["Email"] as string ?? string.Empty,
            Role = r["Role"] as string ?? Roles.Schueler,
            ProfilePicture = r["ProfilePicturePath"] as string ?? "default.png",
            XPPoints = r["XPPoints"] == DBNull.Value ? 0 : Convert.ToInt32(r["XPPoints"]),
            CreatedAt = r["CreatedAt"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["CreatedAt"]),
            IsActive = r["IsActive"] == DBNull.Value || Convert.ToBoolean(r["IsActive"]),
            FirstName = r["FirstName"] as string,
            LastName = r["LastName"] as string,
            ClassName = r["ClassName"] as string
        };
    }
}
