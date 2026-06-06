using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using CybersecurityAwarenessChatbot.Services;
using System.Windows;

namespace CybersecurityAwarenessChatbot
{
    public partial class TaskWindow : Window
    {
        private readonly TaskService taskService = new();
        private readonly MemoryStore memoryStore = new();

        public TaskWindow()
        {
            InitializeComponent();

            UserNameText.Text = $"👤 {memoryStore.UserName}";

            HeaderText.Text = $"Task Assistant - {memoryStore.UserName}";

            LoadTasks();
        }

        private void LoadTasks()
        {
            TaskListBox.ItemsSource =
                taskService.GetTasks();
        }

        private void Refresh_Click(
            object sender,
            RoutedEventArgs e)
        {
            LoadTasks();
        }

        private void AddTask_Click(
            object sender,
            RoutedEventArgs e)
        {
            TaskItem task =
                new TaskItem
                {
                    Title = "Enable 2FA",
                    Description = "Secure accounts",
                    ReminderDate =
                        DateTime.Now.AddDays(7),
                    IsCompleted = false
                };

            taskService.AddTask(task);

            LoadTasks();
        }

        private void Back_Click(
            object sender,
            RoutedEventArgs e)
        {
            new LandingPage().Show();

            Close();
        }
    }
}