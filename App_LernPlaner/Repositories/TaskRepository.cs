using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_LernPlaner.Repositories
{
    public class TaskRepository
    {
        private readonly Database.db_verbindung _db;

        public TaskRepository()
        {
            _db = new Database.db_verbindung();
        }

        public  async Task<List<Models.Task>> GetAllTasks()
        {
            List<Models.Task> tasks = new();

            using (MySqlConnection conn = _db.GetConnection())
            {
               await conn.OpenAsync();

                string query = @"SELECT TaskId ,  UserId , Title , Subject , Deadline , Priority , Status, IsRecurring FROM Tasks";

                using (MySqlCommand cmd = new(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.Default))
                {
                    while (reader.Read())
                    {
                        tasks.Add(new Models.Task
                        {
                            TaskId = Convert.ToInt32(reader["TaskId"]),
                            UserId = Convert.ToInt32(reader["UserId"]),
                            Titel = reader["Title"].ToString(),
                            Subject = reader["Subject"].ToString(),
                            Deadline = Convert.ToDateTime(reader["Deadline"]),
                            Priority = Convert.ToByte(reader["Priority"]),
                            Status = Convert.ToByte(reader["Status"]),
                            IsRecurring = Convert.ToBoolean(reader["IsRecurring"]),
                            ButtonText = "Öffnen"
                        });
                    }
                }
            }

            return tasks;
        }

        
    }
}
