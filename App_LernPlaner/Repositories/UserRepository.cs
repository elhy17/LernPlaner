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
  public class UserRepository
    {
        private readonly db_verbindung _db;
        public UserRepository()
         {

            _db = new db_verbindung();
         }

        public async Task<UserProfile> GetUserProfileByIdAsync(int userId)
        {
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = @"SELECT UserId, Username, PasswordHash, Email, Role, ProfilePicturePath, XPPoints, CreatedAt, FirstName, LastName, ClassName FROM Users WHERE UserId = @userId";

            using MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.Read())
            {
                return null;
            }

            return new UserProfile
            { 
                UserId = Convert.ToInt32(reader["UserId"]),
                Username = reader["Username"]?.ToString() ?? string.Empty,
                Email = reader["Email"]?.ToString() ?? string.Empty,
                Role = reader["Role"]?.ToString() ?? string.Empty,
                ProfilePicturePath = reader["ProfilePicturePath"]?.ToString() ?? string.Empty,
                XPPoints = Convert.ToInt32(reader["XPPoints"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),

                FirstName = reader["FirstName"] == DBNull.Value ? string.Empty : reader["FirstName"].ToString()!,
                LastName = reader["LastName"] == DBNull.Value ? string.Empty : reader["LastName"].ToString()!,
                ClassName = reader["ClassName"] == DBNull.Value ? string.Empty : reader["ClassName"].ToString()!

            };
        
        }

        public async Task UpdateUserProfileAsync(UserProfile profile)
        {
            using MySqlConnection conn = _db.GetConnection();
            await conn.OpenAsync();

            string query = "UPDATE Users SET FirstName=@firstName , LastName=@lastName, Email=@email WHERE UserId = @userId; ";

            using MySqlCommand cmd = new MySqlCommand(query,conn);
            cmd.Parameters.AddWithValue("@userId",profile.UserId);
            cmd.Parameters.AddWithValue("@firstName",profile.FirstName);
            cmd.Parameters.AddWithValue("@lastName", profile.LastName);
            cmd.Parameters.AddWithValue("@email", profile.Email);

            await cmd.ExecuteNonQueryAsync();
        }



    }
}
