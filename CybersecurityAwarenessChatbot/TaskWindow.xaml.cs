/*
    Erwin Mashobane
    ST10073464
*/

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

        // Constructor
        public TaskWindow()
        {
            InitializeComponent();

            UserNameText.Text = $"👤 {MemoryStore.UserName}";
        }

        // Add Task
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

            MessageBox.Show("Task added successfully.");

            LoadTasks();
        }

        // View Tasks
        private void ViewTasks_Click(object sender, RoutedEventArgs e)
        {
            LoadTasks();
        }

        // Load tasks into the ListBox
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

        // Complete tasks
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

        // Delete task
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListBox.SelectedIndex < 0)
                return;

            TaskItem task = taskService.GetTasks() 
                            [TaskListBox.SelectedIndex];

            taskService.DeleteTask(task.Id);

            LoadTasks();
        }


        // Back to landing page
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            LandingPage page = new LandingPage();

            page.Show();

            Close();
        }
    }
}
