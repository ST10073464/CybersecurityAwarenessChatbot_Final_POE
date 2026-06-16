/*
    Erwin Mashobane
    ST10073464
*/

/*
    Handles task storage using JSON.
*/

using CybersecurityAwarenessChatbot.Models;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace CybersecurityAwarenessChatbot.Services
{
    public class DatabaseService
    {
        // Path to JSON file
        private readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                         "Data",
                                         "tasks.json");

        // Constructor ensures data directory and file exist
        public DatabaseService()
        {
            string folder =Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }

            DatabaseService db = new DatabaseService();

            db.AddTask(new TaskItem
            {
                Title = "Test Task",
                Description = "Testing JSON Save",
                ReminderDate = DateTime.Today.AddDays(3),
                IsCompleted = false
            });

        }



        // Add Task
        public void AddTask(TaskItem task)
        {
            MessageBox.Show("AddTask called");

            List<TaskItem> tasks = GetTasks();

            task.Id =
                tasks.Count == 0
                ? 1
                : tasks.Max(t => t.Id) + 1;

            tasks.Add(task);

            SaveTasks(tasks);
        }

        // Get Tasks
        public List<TaskItem> GetTasks()
        {
            if (!File.Exists(filePath))
            {
                return new List<TaskItem>();
            }

            string json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<List<TaskItem>>(json)
                   ?? new List<TaskItem>();
        }

        // Delete Task
        public void DeleteTask(int id)
        {
            List<TaskItem> tasks = GetTasks();

            TaskItem task = tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                tasks.Remove(task);

                SaveTasks(tasks);
            }
        }

        // Complete Task
        public void CompleteTask(int id)
        {
            List<TaskItem> tasks =GetTasks();

            TaskItem task = tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                task.IsCompleted = true;

                SaveTasks(tasks);
            }
        }

        // Save JSON
        private void SaveTasks(List<TaskItem> tasks)
        {
            try
            {
                string json =
                    JsonSerializer.Serialize(
                        tasks,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"JSON Save Error:\n{ex.Message}");
            }
        }
    }
}