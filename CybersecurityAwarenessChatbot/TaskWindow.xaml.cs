/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using CybersecurityAwarenessChatbot.Services;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityAwarenessChatbot
{
    public partial class TaskWindow : Window
    {
        private readonly TaskService taskService;

        private List<TaskItem> tasks;

        private string currentAction = "";

        private string pendingTitle = "";

        private string pendingDescription = "";

        private bool waitingForDescription = false;

        private bool waitingForReminderChoice = false;

        private bool waitingForReminderDate = false;

        private bool awaitingTaskDetails = false;

        private bool awaitingReminderResponse = false;

        private bool awaitingReminderDate;

        private bool awaitingSaveConfirmation;

        //private TaskItem? pendingTask = null;

        private string currentMode = "";

        private TaskItem pendingTask;

        // Constructor.
        public TaskWindow(string mode = "")
        {
            InitializeComponent();

            currentMode = mode;

            taskService = new TaskService();

            tasks = taskService.LoadTasks();

            Loaded += TaskWindow_Loaded;

            AppendBotMessage(MemoryStore.TaskWelcomeMessage);

            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened Task Window");

            ActivityLogTextBox.Text = ActivityLogService.GetLogs("TASK");

            Closing += Window_Closing;

        }

        private void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameText.Text = $"👤 {MemoryStore.UserName}";
            
            // Focus textbox automatically
            UserInputTextBox.Focus();

            LoadActivityLog();
   
        }

        private void LoadActivityLog()
        {
            ActivityLogTextBox.Text = ActivityLogService.GetAllLogs();
        }

        private void AddTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "ADD";

            awaitingTaskDetails = true;

            AppendBotMessage(
                "Please enter:\n\n" +
                "Title | Description");

            ActivityLogService.Add("TASK", $"Task added: '{pendingTask.Title}'");
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

            ActivityLogService.Add("TASK", $"Task completed: '{pendingTask.Title}'");
        }

        private void DeleteTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "DELETE";

            AppendBotMessage("Enter the task title you want to delete.");

            ActivityLogService.Add("TASK", $"Task deleted: '{pendingTask.Title}'");
        }
       private void CheckReminders()
        {
            List<TaskItem> dueTasks = taskService.GetDueReminders();

            foreach (TaskItem task in dueTasks)
            {
                AppendBotMessage($"🔔 Reminder\n\n{task.Title}"
                );
            }

            ActivityLogService.Add("TASK", $"Reminder set for '{pendingTask.Title}' on {pendingTask.ReminderDate:d}");
        }

        // Event handler for the Send button click, which processes the user's input.
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userMessage = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            AppendUserMessage(userMessage);

            SendTaskMessage(userMessage);

            // Clear textbox immediately
            UserInputTextBox.Clear();
        }

        // Core logic for handling user input based on the current action (ADD, COMPLETE, DELETE).
        private void SendTaskMessage(string message)
        {
            AppendUserMessage(message);

            // STEP 1: User entered task title
            if (!waitingForDescription &&
                !waitingForReminderChoice &&
                !waitingForReminderDate)
            {
                pendingTitle = message;

                waitingForDescription = true;

                AppendBotMessage(
                    $"Task title saved:\n\n" +
                    $"📋 {pendingTitle}\n\n" +
                    $"Please provide a description for this task.");

                return;
            }

            // STEP 2: User enters description
            if (waitingForDescription)
            {
                pendingDescription = message;

                waitingForDescription = false;
                waitingForReminderChoice = true;

                AppendBotMessage(
                    $"Task added with the description:\n\n" +
                    $"\"{pendingDescription}\"\n\n" +
                    $"Would you like a reminder? (Yes/No)");

                return;
            }

            // STEP 3: User chooses reminder
            if (waitingForReminderChoice)
            {
                waitingForReminderChoice = false;

                pendingTask = new TaskItem
                {
                    Title = pendingTitle,
                    Description = pendingDescription,
                    IsCompleted = false
                };

                if (message.ToLower().Contains("yes"))
                {
                    waitingForReminderDate = true;

                    AppendBotMessage(
                        "Great! When should I remind you?\n\n" +
                        "Examples:\n" +
                        "• 3 days\n" +
                        "• 7 days\n" +
                        "• tomorrow");

                    return;
                }

                pendingTask.ReminderDate = DateTime.MinValue;

                taskService.AddTask(pendingTask);

                ActivityLogService.Add("TASK", $"{MemoryStore.UserName} created task: {pendingTask.Title}");

                MessageBoxResult result = MessageBox.Show(
                                    $"✅ Task Added Successfully!\n\n" +
                                    $"Task: {pendingTask.Title}\n\n" +
                                    $"Reminder Date: {pendingTask.ReminderDate:d}",
                                    "Task Saved",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                if (result == MessageBoxResult.OK)
                {
                    DatabaseService db = new DatabaseService();

                    db.AddTask(pendingTask);

                    ActivityLogService.Add("TASK",
                        $"{MemoryStore.UserName} added task '{pendingTask.Title}'");

                    AppendBotMessage(
                        $"✅ Task saved successfully!\n\n" +
                        $"📋 {pendingTask.Title}\n\n" +
                        $"⏰ Reminder set for:\n" +
                        $"{pendingTask.ReminderDate:d}");
                }

                return;
            }
        }
      
        // Event handler for pressing Enter in the UserInputTextBox.
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendTaskMessage(UserInputTextBox.Text);

                UserInputTextBox.Clear();

                e.Handled = true; // prevents the beep sound
            }
        }

        // Event handler for text changes in the UserInputTextBox to manage placeholder visibility.
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                                       ? Visibility.Visible
                                       : Visibility.Hidden;
        }

        // Event handler for when the user selects a date from the ReminderDatePicker.
        private void ReminderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!awaitingReminderDate || pendingTask == null)
                return;

            if (ReminderDatePicker.SelectedDate.HasValue)
            {
                pendingTask.ReminderDate = ReminderDatePicker.SelectedDate.Value;

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

        private void ViewJsonTasksButton_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "tasks.json");

            if (!File.Exists(path))
            {
                MessageBox.Show("No saved tasks found.",
                                "Tasks",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            string json = File.ReadAllText(path);

            MessageBox.Show(json,
                            "Saved Tasks (JSON)",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} viewed saved JSON tasks");
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

        private void Window_Closing(object sender,CancelEventArgs e)
        {
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} closed Task Window");
        }

        // Back to main chat window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow chatWindow = new MainWindow();

            chatWindow.Show();

            Close();
        }
    }

}
