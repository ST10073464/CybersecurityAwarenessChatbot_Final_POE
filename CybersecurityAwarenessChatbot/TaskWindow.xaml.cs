/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.Json;

namespace CybersecurityAwarenessChatbot
{
    public partial class TaskWindow : Window
    {
        private readonly TaskService taskService;

        private List<TaskItem> tasks;

        private string currentAction = "";

        private bool awaitingTaskDetails = false;

        private bool awaitingReminderResponse = false;

        private bool awaitingReminderDate;

        private bool awaitingSaveConfirmation;

        private TaskItem? pendingTask = null;

        private string StartupMode = "";



        public TaskWindow(string mode = "")
        {
            InitializeComponent();

            taskService = new TaskService();

            StartupMode = mode;

            taskService = new TaskService();

            tasks = taskService.LoadTasks();

            Loaded += TaskWindow_Loaded;

            taskService.AddTask(pendingTask);

            ActivityLogService.Add($"Task Created: {pendingTask.Title}");

            ActivityLogService.Add($"Task Deleted: {pendingTask.Title}");

            ActivityLogService.Add($"{MemoryStore.UserName} viewed tasks");

            ActivityLogService.Add($"Reminder Set: {pendingTask.Title} - {pendingTask.ReminderDate:d}");


            //ActivityLogService.Add($"Task added: '{pendingTask.Title}'");

            //ActivityLogService.Add($"Reminder set for '{pendingTask.Title}' on {pendingTask.ReminderDate:d}");

        }

        private void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameText.Text = $"👤 {MemoryStore.UserName}";
            
            // Focus textbox automatically
            UserInputTextBox.Focus();

            LoadActivityLog();

            ShowWelcomeMenu();

            //CheckReminders();
        }

        private void LoadActivityLog()
        {
            TaskActivityLogText.Text =
                ActivityLogService.GetSummary();
        }

        private void ShowWelcomeMenu()
        {
            AppendBotMessage($"👋 Welcome {MemoryStore.UserName}\n\n" +
                             $"Cybersecurity Task Assistant Activated.\n\n" +
                             $"Please choose one of the following options:"
            );
        }

        private void AddTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "ADD";

            awaitingTaskDetails = true;

            AppendBotMessage(
                "Please enter:\n\n" +
                "Title | Description");
        }

        private void ViewTasksOption_Click(object sender, RoutedEventArgs e)
        {
            tasks = taskService.LoadTasks();

            if (!tasks.Any())
            {
                AppendBotMessage("📋 No tasks available.");
                return;
            }

            string response = "📋 TASK LIST\n\n";

            foreach (TaskItem task in tasks)
            {
                response +=
                    $"• {task.Title}\n" +
                    $"  {task.Description}\n" +
                    $"  Status: {(task.IsCompleted ? "✅ Completed" : "⏳ Pending")}\n\n";
            }

            AppendBotMessage(response);
        }

        private void CompleteTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "COMPLETE";

            AppendBotMessage("Enter the task title you want to complete.");
        }

        private void DeleteTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "DELETE";

            AppendBotMessage("Enter the task title you want to delete.");
        }
        /*private void CheckReminders()
        {
            List<TaskItem> dueTasks = taskService.GetDueReminders();

            foreach (TaskItem task in dueTasks)
            {
                AppendBotMessage($"🔔 Reminder\n\n{task.Title}"
                );
            }
        }
        */
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                                       ? Visibility.Visible
                                       : Visibility.Hidden;
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendTaskMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendTaskMessage();
        }

        // Core logic for handling user input based on the current action (ADD, COMPLETE, DELETE).
        private void SendTaskMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            AppendUserMessage(input);

            string lowerInput = input.ToLower();

            // NLP ADD TASK

            if (lowerInput.Contains("add task") ||
                lowerInput.Contains("create task") ||
                lowerInput.Contains("new task"))
            {
                awaitingTaskDetails = true;

                AppendBotMessage("Please enter:\n\n" +
                                 "Title | Description");

                UserInputTextBox.Clear();
                return;
            }

            // NLP VIEW TASKS

            if (lowerInput.Contains("view task") ||
                lowerInput.Contains("show task") ||
                lowerInput.Contains("list task"))
            {
                AppendBotMessage(taskService.GetTaskSummary());

                UserInputTextBox.Clear();
                return;
            }

            // NLP COMPLETE TASK

            if (lowerInput.Contains("complete task") ||
                lowerInput.Contains("finish task") ||
                lowerInput.Contains("mark task"))
            {
                currentAction = "COMPLETE";

                AppendBotMessage(
                    "Enter the title of the task to complete.");

                UserInputTextBox.Clear();
                return;
            }

            // NLP DELETE TASK

            if (lowerInput.Contains("delete task") ||
                lowerInput.Contains("remove task") ||
                lowerInput.Contains("cancel task"))
            {
                currentAction = "DELETE";

                AppendBotMessage(
                    "Enter the title of the task to delete.");

                UserInputTextBox.Clear();
                return;
            }

            // SUMMARY

            if (lowerInput.Contains("what have you done") ||
                lowerInput.Contains("recent actions") ||
                lowerInput.Contains("activity"))
            {
                AppendBotMessage(taskService.GetTaskSummary());

                UserInputTextBox.Clear();
                return;
            }

            AppendBotMessage("I didn't quite understand.\n\n" +
                             "Try:\n" +
                             "• Add Task\n" +
                             "• View Tasks\n" +
                             "• Complete Task\n" +
                             "• Delete Task");

            UserInputTextBox.Clear();
        }

        // Event handler for when the user selects a date from the ReminderDatePicker.
        private void ReminderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!awaitingReminderDate || pendingTask == null)
                return;

            if (ReminderDatePicker.SelectedDate.HasValue)
            {
                pendingTask.ReminderDate =
                    ReminderDatePicker.SelectedDate.Value;

                awaitingReminderDate = false;

                awaitingSaveConfirmation = true;

                ReminderDatePicker.Visibility = Visibility.Collapsed;

                AppendBotMessage(
                    $"📅 Reminder set successfully.\n\n" +
                    $"Task Details:\n\n" +
                    $"Title: {pendingTask.Title}\n" +
                    $"Description: {pendingTask.Description}\n" +
                    $"Reminder: {pendingTask.ReminderDate:dd MMMM yyyy}\n\n" +
                    $"Would you like to save this task?\n\n" +
                    $"Type:\n" +
                    $"Yes\n" +
                    $"No");
            }
        }

        private void ShowSavedJsonTask()
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "tasks.json");

            string json = File.ReadAllText(path);

            AppendBotMessage("📁 Current JSON File:\n\n" + json);
        }

        private void AppendBotMessage(string message)
        {
            StackPanel container = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5)
            };

            TextBlock header = new TextBlock
            {
                Text = $"🤖 Task Assistant • {DateTime.Now:HH:mm}",
                Foreground = Brushes.Gray,
                FontSize = 13,
                FontWeight = FontWeights.Bold
            };

            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 38, 58)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                MaxWidth = 700
            };

            TextBlock text = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            };

            bubble.Child = text;

            container.Children.Add(header);
            container.Children.Add(bubble);

            ChatPanel.Children.Add(container);

            ChatScrollViewer.ScrollToBottom();
        }

        // Displays username and timestamp, with a double tick indicator for sent messages.
        private void AppendUserMessage(string message)
        {
            StackPanel container = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            // Username and timestamp
            TextBlock header = new TextBlock
            {
                Text = $"{MemoryStore.UserName} • {DateTime.Now:HH:mm}",
                Foreground = Brushes.Gray,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 194, 255)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                MaxWidth = 700
            };

            StackPanel bubbleContent = new StackPanel();

            TextBlock text = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            };

            // Double tick indicator
            TextBlock ticks = new TextBlock
            {
                Text = "✓✓",
                Foreground = Brushes.White,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };

            bubbleContent.Children.Add(text);
            bubbleContent.Children.Add(ticks);

            bubble.Child = bubbleContent;

            container.Children.Add(header);
            container.Children.Add(bubble);

            ChatPanel.Children.Add(container);
        }

        // Back to main chat window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow();

            chatWindow.Show();

            Close();
        }
    }

}
