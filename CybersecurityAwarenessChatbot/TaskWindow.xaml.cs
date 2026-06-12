using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using CybersecurityAwarenessChatbot.Services;
using System.Windows;

namespace CybersecurityAwarenessChatbot
{
    public partial class TaskWindow : Window
    {
        private readonly TaskService taskService = new TaskService();

        private readonly MemoryStore memoryStore = new();

        public TaskWindow()
        {
            InitializeComponent();

            UserNameText.Text = $"👤 {MemoryStore.UserName}";
        }

        // ADD TASK
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            TaskItem task = new TaskItem
            {
                Title = TitleTextBox.Text,
                Description = DescriptionTextBox.Text,
                ReminderDate = ReminderDatePicker.SelectedDate,
                IsCompleted = false
            };

            taskService.AddTask(task);

            MessageBox.Show(
                "Task added successfully.");

            LoadTasks();
        }

        // VIEW TASKS
        private void ViewTasks_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadTasks();
        }

        // LOAD TASKS
        private void LoadTasks()
        {
            TaskListBox.Items.Clear();

            foreach (TaskItem task
                in taskService.GetTasks())
            {
                TaskListBox.Items.Add(
                    $"ID: {task.Id} | " +
                    $"{task.Title} | " +
                    $"Completed: {task.IsCompleted}");
            }
        }

        // COMPLETE TASK
        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedIndex < 0)
                return;

            TaskItem task =
                taskService.GetTasks()
                [TaskListBox.SelectedIndex];

            taskService.CompleteTask(task.Id);

            LoadTasks();
        }

        // Delete TASK
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedIndex < 0)
                return;

            TaskItem task = taskService.GetTasks() 
                            [TaskListBox.SelectedIndex];

            taskService.DeleteTask(task.Id);

            LoadTasks();
        }

        // BACK TO LANDING PAGE
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            LandingPage page = new LandingPage();

            page.Show();

            Close();
        }
    }
}
