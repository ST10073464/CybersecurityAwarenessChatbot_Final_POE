/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Models;
using Newtonsoft.Json;
using System.IO;

namespace CybersecurityAwarenessChatbot.Classes
{
    public class TaskService
    {
        private readonly string filePath =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "tasks.json");

        public TaskService()
        {
            string folder =
                Path.GetDirectoryName(filePath);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
            }
        }

        // LOAD TASKS
        public List<TaskItem> LoadTasks()
        {
            try
            {
                string json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<TaskItem>();

                List<TaskItem>? tasks =
                    JsonConvert.DeserializeObject<List<TaskItem>>(json);

                return tasks ?? new List<TaskItem>();
            }
            catch
            {
                return new List<TaskItem>();
            }
        }

        // SAVE TASKS
        public void SaveTasks(List<TaskItem> tasks)
        {
            string json =
                JsonConvert.SerializeObject(
                    tasks,
                    Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(filePath, json);
        }

        // ADD TASK
        public void AddTask(TaskItem task)
        {
            List<TaskItem> tasks = LoadTasks();

            tasks.Add(task);

            SaveTasks(tasks);
        }

        // DELETE TASK
        public bool DeleteTask(string title)
        {
            List<TaskItem> tasks = LoadTasks();

            TaskItem task =
                tasks.FirstOrDefault(t =>
                    t.Title.Equals(
                        title,
                        StringComparison.OrdinalIgnoreCase));

            if (task == null)
                return false;

            tasks.Remove(task);

            SaveTasks(tasks);

            return true;
        }

        // COMPLETE TASK
        public bool MarkTaskCompleted(string title)
        {
            List<TaskItem> tasks = LoadTasks();

            TaskItem task =
                tasks.FirstOrDefault(t =>
                    t.Title.Equals(
                        title,
                        StringComparison.OrdinalIgnoreCase));

            if (task == null)
                return false;

            task.IsCompleted = true;

            SaveTasks(tasks);

            return true;
        }


        // DUE REMINDERS
        public List<TaskItem> GetDueReminders()
        {
            return LoadTasks()
                .Where(t =>
                    t.ReminderDate.HasValue &&
                    !t.IsCompleted &&
                    t.ReminderDate.Value.Date <= DateTime.Today)
                .ToList();
        }
    }
}