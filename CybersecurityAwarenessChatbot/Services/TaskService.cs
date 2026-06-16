/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Models;
using System.IO;
using System.Text.Json;

namespace CybersecurityAwarenessChatbot.Classes
{
    // Service for managing tasks with JSON storage.
    public class TaskService
    {
        private readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tasks.json");

        public TaskService()
        {
            string? folder = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder!);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
        }

        // Load TASKS
        public List<TaskItem> LoadTasks()
        {
            try
            {
                string json = File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        // Save TASKS
        private void SaveTasks(List<TaskItem> tasks)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

            string json = JsonSerializer.Serialize(tasks, options);

            File.WriteAllText(filePath, json);
        }

        // Add TASK
        public void AddTask(TaskItem task)
        {
            List<TaskItem> tasks = LoadTasks();

            tasks.Add(task);

            string json =
                JsonSerializer.Serialize(
                    tasks,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

            File.WriteAllText(filePath, json);

        }

        // Complete TASK
        public bool CompleteTask(string title)
        {
            List<TaskItem> tasks = LoadTasks();

            TaskItem? task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (task == null)
                return false;

            task.IsCompleted = true;

            SaveTasks(tasks);

            return true;
        }

        // Delete TASK
        public bool DeleteTask(string title)
        {
            List<TaskItem> tasks = LoadTasks();

            TaskItem? task = tasks.FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (task == null)
                return false;

            tasks.Remove(task);

            SaveTasks(tasks);

            return true;
        }

        // Find TASK

        public TaskItem? FindTask(string title)
        {
            return LoadTasks().FirstOrDefault(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        // Reminders Due Today
       // public List<TaskItem> GetDueReminders()
       // {
       //     //return LoadTasks().Where(t =>
                    //!t.IsCompleted &&
                    //t.ReminderDate.HasValue &&
        //            //t.ReminderDate.Value.Date <= DateTime.Today).ToList();
       // }

        // view TASKS as text summary
        public string GetTaskSummary()
        {
            List<TaskItem> tasks = LoadTasks();

            if (!tasks.Any())
            {
                return "📋 No tasks available.";
            }

            string result = "📋 Your Cybersecurity Tasks\n\n";

            foreach (TaskItem task in tasks)
            {
                string status =
                    task.IsCompleted
                    ? "✅ Completed"
                    : "⏳ Pending";

                string reminder =
                    task.ReminderDate.HasValue
                    ? task.ReminderDate.Value.ToShortDateString()
                    : "No Reminder";

                result +=
                    $"Title: {task.Title}\n" +
                    $"Description: {task.Description}\n" +
                    $"Reminder: {reminder}\n" +
                    $"Status: {status}\n\n";
            }

            return result;
        }
    }
}