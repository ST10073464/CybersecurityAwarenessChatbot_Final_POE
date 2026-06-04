/*
    Erwin Mashobane
    ST10073464
*/

using MySql.Data.MySqlClient;
using CybersecurityAwarenessChatbot.Models;

namespace CybersecurityAwarenessChatbot.Services
{
    public class DatabaseService
    {
        private readonly string connectionString =
            "server=localhost;" +
            "database=Cybersecurityawarenesschatbot;" +
            "uid=root;" +
            "pwd=root;";

        // Add Task
        public void AddTask(TaskItem task)
        {
            using MySqlConnection connection =
                new MySqlConnection(connectionString);

            connection.Open();

            string query =
                @"INSERT INTO Tasks
                (Title, Description, ReminderDate, IsCompleted)
                VALUES
                (@Title, @Description, @ReminderDate, @IsCompleted)";

            MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Title", task.Title);
            command.Parameters.AddWithValue("@Description", task.Description);
            command.Parameters.AddWithValue("@ReminderDate", task.ReminderDate);
            command.Parameters.AddWithValue("@IsCompleted", task.IsCompleted);

            command.ExecuteNonQuery();
        }

        // Get All Tasks
        public List<TaskItem> GetTasks()
        {
            List<TaskItem> tasks = new();

            using MySqlConnection connection =
                new MySqlConnection(connectionString);

            connection.Open();

            string query = "SELECT * FROM Tasks";

            MySqlCommand command =
                new MySqlCommand(query, connection);

            MySqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(new TaskItem
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    Title = reader["Title"].ToString(),
                    Description = reader["Description"].ToString(),
                    ReminderDate =
                        reader["ReminderDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["ReminderDate"]),
                    IsCompleted =
                        Convert.ToBoolean(reader["IsCompleted"])
                });
            }

            return tasks;
        }

        // Delete Task
        public void DeleteTask(int id)
        {
            using MySqlConnection connection =
                new MySqlConnection(connectionString);

            connection.Open();

            string query =
                "DELETE FROM Tasks WHERE Id=@Id";

            MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }

        // Mark Completed
        public void CompleteTask(int id)
        {
            using MySqlConnection connection =
                new MySqlConnection(connectionString);

            connection.Open();

            string query =
                @"UPDATE Tasks
                  SET IsCompleted = 1
                  WHERE Id=@Id";

            MySqlCommand command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            command.ExecuteNonQuery();
        }
    }
}