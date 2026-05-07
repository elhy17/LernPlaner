using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MySql.Data.MySqlClient;

namespace App_LernPlaner.Services
{


    public class logIn
    {
        public bool IsSuccessful { get; private set; }
        public string ErrorMessage { get; private set; }

        public logIn(string username, string password)
        {
            IsSuccessful = false;

            try
            {
                datenbankServices ds = new datenbankServices();

                using (var conn = new MySqlConnection(ds.getConnStrg()))
                {
                    conn.Open();

                    string query = "SELECT PasswordHash FROM Users WHERE Username = @Username;";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString("PasswordHash");

                                if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                                {
                                    IsSuccessful = true;
                                }
                                else
                                {
                                    ErrorMessage = "Falsches Passwort";
                                    MessageBox.Show(ErrorMessage);

                                }
                            }
                            else
                            {
                                ErrorMessage = "User nicht gefunden";
                                MessageBox.Show(ErrorMessage);

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Datenbank Error " + ex.Message;
                MessageBox.Show(ErrorMessage);
            }
        }
    

    }
}
