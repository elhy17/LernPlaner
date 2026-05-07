using System;
using App_LernPlaner.Services;
using MySql.Data.MySqlClient; 

namespace App_LernPlaner.Models
{
    public class addUser
    {
        public bool IsSuccessful { get; private set; }
        public string Message { get; private set; }

        public addUser(string username, string passwordHash, string email, string role, string profilePicture, int xpPoints)
        {
            IsSuccessful = false; 

            try
            {
                datenbankServices ds = new datenbankServices();

                using (var conn = new MySqlConnection(ds.getConnStrg()))
                {
                    conn.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";

                    using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);

                        int userCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (userCount > 0)
                        {
                            Message = "ein Konto mit die Gleiche Daten ist schon angelegt !, bitte melden Sie sich an";
                            return; 
                        }
                    }

                    string insertQuery = @"INSERT INTO Users 
                                           (Username, PasswordHash, Email, Role, ProfilePicturePath, XPPoints, CreatedAt) 
                                           VALUES 
                                           (@Username, @PasswordHash, @Email, @Role, @ProfilePicture, @XPPoints, @CreatedAt)";

                    using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@Username", username);
                        insertCmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        insertCmd.Parameters.AddWithValue("@Email", email);
                        insertCmd.Parameters.AddWithValue("@Role", role);
                        insertCmd.Parameters.AddWithValue("@ProfilePicture", profilePicture);
                        insertCmd.Parameters.AddWithValue("@XPPoints", xpPoints);
                        insertCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        insertCmd.ExecuteNonQuery();

                        IsSuccessful = true;
                        Message = "Registrierung Erfolgreich";
                    }
                }
            }
            catch (Exception ex)
            {
                Message = "datenBank error " + ex.Message;
            }
        }
    }
}