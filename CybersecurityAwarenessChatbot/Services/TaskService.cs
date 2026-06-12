/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Models;

namespace CybersecurityAwarenessChatbot.Services
{
    // TaskService handles task management operations.
    public class TaskService
    {
        private readonly DatabaseService database;

        public TaskService()
        {
            database = new DatabaseService();
        }

        public void AddTask(TaskItem task)
        {
            database.AddTask(task);
        }

        public List<TaskItem> GetTasks()
        {
            return database.GetTasks();
        }

        public void DeleteTask(int id)
        {
            database.DeleteTask(id);
        }

        public void CompleteTask(int id)
        {
            database.CompleteTask(id);
        }
    }
}