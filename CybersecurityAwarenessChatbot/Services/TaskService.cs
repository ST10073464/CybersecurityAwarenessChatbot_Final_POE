namespace CybersecurityAwarenessChatbot.Classes
{
    public class TaskService
    {
        private readonly List<TaskItem> tasks = new();

        public void AddTask(TaskItem task)
        {
            tasks.Add(task);
        }

        public List<TaskItem> GetTasks()
        {
            return tasks;
        }

        public void CompleteTask(TaskItem task)
        {
            task.IsCompleted = true;
        }

        public void DeleteTask(TaskItem task)
        {
            tasks.Remove(task);
        }
    }
}