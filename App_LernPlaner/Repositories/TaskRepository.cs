using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

        public async Task<List<Models.StudyTask>> GetAllTasks()
        {
            List<Models.StudyTask> tasks = new();

            using (MySqlConnection conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"SELECT TaskId ,  UserId , Title , Subject , Deadline , Priority , Status, IsRecurring, ClassId, CreatedBy FROM Tasks ORDER BY Deadline ASC";

                using (MySqlCommand cmd = new(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.Default))
                {
                    while (reader.Read())
                    {
                        tasks.Add(MapTask(reader));
                    }
                }
            }

            return tasks;
        }

        // Get Task by User ID
        
        public async Task<List<Models.StudyTask>> GetTasksByUserIdAsync(int UserId)
        {
            List<Models.StudyTask> tasks = new();

            using (MySqlConnection conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @" SELECT 
                                t.TaskId,
                                t.UserId,
                                t.Title,
                                t.Subject,
                                t.Deadline,
                                t.Priority,
                                COALESCE(s.Status, t.Status) AS Status,
                                t.IsRecurring,
                                t.ClassId,
                                t.CreatedBy
                            FROM Tasks t
                            LEFT JOIN Submissions s
                            ON s.TaskId = t.TaskId
                            AND s.UserId = @userId
                            WHERE t.UserId = @userId
                            ORDER BY t.Deadline ASC";
                using (MySqlCommand cmd = new MySqlCommand(query,conn))
                {
                    cmd.Parameters.AddWithValue("UserId",UserId);

                    using (var reader = await cmd.ExecuteReaderAsync(CommandBehavior.Default))
                    {
                        while (reader.Read())
                        {
                            tasks.Add(MapTask(reader));
                        }
                    }

                }
            }

            return tasks; 
        }





        // Task hinzufügen 

        public async Task HinzufugenTask(Models.StudyTask task)
        {
            using (MySqlConnection conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                var query = @"INSERT INTO Tasks
                        (UserId, Title, Subject, Deadline, Priority, Status, IsRecurring, ClassId, CreatedBy)
                    VALUES
                        (@userId, @title, @sub, @dead, @prio, @status, @isRecu, @classId, @createdBy)";

                using(MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@taskId", task.TaskId);
                    cmd.Parameters.AddWithValue("@userId", task.UserId);
                    cmd.Parameters.AddWithValue("@title", task.Titel);
                    cmd.Parameters.AddWithValue("@sub", task.Subject);
                    cmd.Parameters.AddWithValue("@Dead", task.Deadline);
                    cmd.Parameters.AddWithValue("@Prio", task.Priority);
                    cmd.Parameters.AddWithValue("@status", task.Status);
                    cmd.Parameters.AddWithValue("@isRecu", task.IsRecurring);
                    cmd.Parameters.AddWithValue("@classId", task.ClassId);
                    cmd.Parameters.AddWithValue("@createdby", task.CreatedBy);

                    await cmd.ExecuteNonQueryAsync();
                }

              


            }
        }

        // Status ändern

        public async Task StatusAendernAsync(int taskId, byte neuerStatus)
        {
            using (MySqlConnection conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
                    UPDATE Tasks
                    SET Status = @status
                    WHERE TaskId = @taskId";

                using (MySqlCommand cmd = new MySqlCommand(query, conn)){

                    cmd.Parameters.AddWithValue("@status", neuerStatus);
                    cmd.Parameters.AddWithValue("@taskId", taskId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

        }

        // Löschen TAsk mit ID
        public async Task LoeschenAsync(int taskId)
        {
            using (MySqlConnection conn = _db.GetConnection())
            {
                await conn.OpenAsync();

                string query = @"
                    DELETE FROM Tasks
                    WHERE TaskId = @taskId";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                { 

                    cmd.Parameters.AddWithValue("@taskId", taskId);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // Hadi MappTask
        private Models.StudyTask MapTask(IDataRecord reader)
        {
            return new Models.StudyTask
            {
                TaskId = Convert.ToInt32(reader["TaskId"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                Titel = reader["Title"]?.ToString() ?? "",
                Subject = reader["Subject"]?.ToString() ?? "",
                Deadline = Convert.ToDateTime(reader["Deadline"]),
                Priority = Convert.ToByte(reader["Priority"]),
                Status = Convert.ToByte(reader["Status"]),
                IsRecurring = Convert.ToBoolean(reader["IsRecurring"]),
                ClassId = reader["ClassId"] == DBNull.Value ? null : Convert.ToInt32(reader["ClassId"]),
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(reader["CreatedBy"]),
                ButtonText = "Öffnen"
            };
        }


    }
}
