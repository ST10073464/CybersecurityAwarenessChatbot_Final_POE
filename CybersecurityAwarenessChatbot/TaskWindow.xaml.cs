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

        public TaskWindow()
        {
            InitializeComponent();

            taskService = new TaskService();

            tasks = taskService.LoadTasks();

            Loaded += TaskWindow_Loaded;
        }

        private void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            ShowWelcomeMenu();

            CheckReminders();
        }

        private void ShowWelcomeMenu()
        {
            AppendBotMessage(
                $"👋 Welcome {MemoryStore.UserName}\n\n" +
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

            AppendBotMessage(
                "Enter the task title you want to complete.");
        }

        private void DeleteTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "DELETE";

            AppendBotMessage(
                "Enter the task title you want to delete.");
        }
        private void CheckReminders()
        {
            List<TaskItem> dueTasks = taskService.GetDueReminders();

            foreach (TaskItem task in dueTasks)
            {
                AppendBotMessage($"🔔 Reminder\n\n{task.Title}"
                );
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow();

            chatWindow.Show();

            Close();
        }

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

            // ============================================
            // ADD TASK
            // ============================================

            if (awaitingTaskDetails)
            {
                string[] parts = input.Split('|');

                if (parts.Length < 2)
                {
                    AppendBotMessage(
                        "❌ Invalid format.\n\n" +
                        "Use:\n\n" +
                        "Title | Description");

                    UserInputTextBox.Clear();
                    return;
                }

                pendingTask = new TaskItem
                {
                    Title = parts[0].Trim(),
                    Description = parts[1].Trim(),
                    IsCompleted = false
                };

                awaitingTaskDetails = false;
                awaitingReminderDate = true;

                ReminderDatePicker.Visibility = Visibility.Visible;

                AppendBotMessage(
                    "✅ Task created.\n\n" +
                    "Please select a reminder date using the Date Picker.");

                UserInputTextBox.Clear();
                return;
            }

            // ============================================
            // SAVE CONFIRMATION
            // ============================================

            if (awaitingSaveConfirmation && pendingTask != null)
            {
                string lower = input.ToLower().Trim();

                if (lower.Contains("yes"))
                {
                    taskService.AddTask(pendingTask);

                    AppendBotMessage(
                        "✅ Task saved successfully.");

                    ShowSavedJsonTask();

                    pendingTask = null;
                    awaitingSaveConfirmation = false;
                }
                else if (lower.Contains("no"))
                {
                    awaitingTaskDetails = true;
                    awaitingSaveConfirmation = false;

                    AppendBotMessage(
                        "✏️ Please enter the updated task.\n\n" +
                        "Title | Description");
                }
                else
                {
                    AppendBotMessage(
                        "Please reply:\n\n" +
                        "Yes - Save Task\n" +
                        "No - Edit Task");
                }

                UserInputTextBox.Clear();
                return;
            }

            // ============================================
            // COMPLETE TASK
            // ============================================

            if (currentAction == "COMPLETE")
            {
                bool success = taskService.CompleteTask(input);

                AppendBotMessage(
                    success
                    ? $"✅ Task '{input}' marked as completed."
                    : $"❌ Task '{input}' not found.");

                currentAction = "";

                UserInputTextBox.Clear();
                return;
            }

            // ============================================
            // DELETE TASK
            // ============================================

            if (currentAction == "DELETE")
            {
                bool success = taskService.DeleteTask(input);

                AppendBotMessage(
                    success
                    ? $"🗑 Task '{input}' deleted."
                    : $"❌ Task '{input}' not found.");

                currentAction = "";

                UserInputTextBox.Clear();
                return;
            }

            // ============================================
            // VIEW TASKS
            // ============================================

            if (input.Equals("view tasks",
                StringComparison.OrdinalIgnoreCase))
            {
                AppendBotMessage(taskService.GetTaskSummary());
            }
            else
            {
                AppendBotMessage(
                    "Please select an option from the left menu.");
            }

            UserInputTextBox.Clear();
        }

        // Event handler for when the user selects a date from the ReminderDatePicker.
        private void ReminderDatePicker_SelectedDateChanged(
      object sender,
      SelectionChangedEventArgs e)
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

        private void AppendUserMessage(string message)
        {
            StackPanel container = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 194, 255)),
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

            container.Children.Add(bubble);

            ChatPanel.Children.Add(container);

            ChatScrollViewer.ScrollToBottom();
        }
    }

}
