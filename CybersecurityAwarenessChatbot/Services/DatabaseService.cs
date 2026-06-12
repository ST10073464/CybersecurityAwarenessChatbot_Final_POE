/*
    Erwin Mashobane
    ST10073464
*/

/*
    Handles task storage using JSON.
*/

using System.IO;
using System.Text.Json;
using CybersecurityAwarenessChatbot.Models;

namespace CybersecurityAwarenessChatbot.Services
{
    public class DatabaseService
    {
        // Path to JSON file
        private readonly string filePath = "Data/tasks.json";

        // Add Task
        public void AddTask(TaskItem task)
        {
            List<TaskItem> tasks = GetTasks();

            // Generate next ID
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

            string json =
                File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<List<TaskItem>>(json)
                   ?? new List<TaskItem>();
        }

        // Delete Task
        public void DeleteTask(int id)
        {
            List<TaskItem> tasks =
                GetTasks();

            TaskItem task =
                tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                tasks.Remove(task);

                SaveTasks(tasks);
            }
        }

        // Complete Task
        public void CompleteTask(int id)
        {
            List<TaskItem> tasks =
                GetTasks();

            TaskItem task =
                tasks.FirstOrDefault(t => t.Id == id);

            if (task != null)
            {
                task.IsCompleted = true;

                SaveTasks(tasks);
            }
        }

        // Save JSON
        private void SaveTasks(
            List<TaskItem> tasks)
        {
            string json =
                JsonSerializer.Serialize(
                    tasks,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(
                filePath,
                json);
        }
    }
}