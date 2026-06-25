/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using CybersecurityAwarenessChatbot.Services; 


public class TaskManager
{
    private readonly TaskService taskService;

    public TaskManager()
    {
        taskService = new TaskService();
    }

    public string AddTask(TaskItem task)
    {
        taskService.AddTask(task);

        ActivityLogService.Add("TASK", $"Task added: {task.Title}");

        return $"✅ Task '{task.Title}' added successfully.";
    }

    public List<TaskItem> GetAllTasks()
    {
        return taskService.LoadTasks();
    }

    public bool MarkAsComplete(string title)
    {
        List<TaskItem> tasks = taskService.LoadTasks();

        TaskItem task =
            tasks.FirstOrDefault(t =>
                t.Title.Equals(title,
                StringComparison.OrdinalIgnoreCase));

        if (task == null)
            return false;

        task.IsCompleted = true;

        taskService.SaveTasks(tasks);

        ActivityLogService.Add(
            "TASK",
            $"Completed task: {task.Title}");

        return true;
    }

    public bool DeleteTask(string title)
    {
        List<TaskItem> tasks = taskService.LoadTasks();

        TaskItem task =
            tasks.FirstOrDefault(t =>
                t.Title.Equals(title,
                StringComparison.OrdinalIgnoreCase));

        if (task == null)
            return false;

        tasks.Remove(task);

        taskService.SaveTasks(tasks);

        ActivityLogService.Add(
            "TASK",
            $"Deleted task: {task.Title}");

        return true;
    }
}