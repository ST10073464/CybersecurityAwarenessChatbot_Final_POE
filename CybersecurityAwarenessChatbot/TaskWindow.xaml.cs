/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityAwarenessChatbot
{
    public partial class TaskWindow : Window
    {
        private readonly TaskService taskService;

        private MainWindow chatWindow;

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

        private string currentMode = "";

        private TaskItem pendingTask;

        // Constructor.
        public TaskWindow(string mode = "", MainWindow mainWindow = null)
        {
            InitializeComponent();

            chatWindow = mainWindow;

            currentMode = mode;

            taskService = new TaskService();

            tasks = taskService.LoadTasks();

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

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
            string title =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter task title:",
                    "Add Task");

            if (string.IsNullOrWhiteSpace(title))
                return;

            string description =
                Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter task description:",
                    "Add Task");

            if (string.IsNullOrWhiteSpace(description))
                return;

            MessageBoxResult result =
                MessageBox.Show(
                    "Would you like to add a reminder?",
                    "Reminder",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            DateTime reminderDate = DateTime.MinValue;

            bool hasReminder = false;

            if (result == MessageBoxResult.Yes)
            {
                hasReminder = true;

                string daysInput =Microsoft.VisualBasic.Interaction.InputBox(
                        "Remind me in how many days?",
                        "Reminder");

                if (int.TryParse(daysInput, out int days))
                {
                    reminderDate = DateTime.Today.AddDays(days);

                    hasReminder = true;
                }
            }

            pendingTask = new TaskItem
            {
                Title = title,
                Description = description,
                ReminderDate = reminderDate,
                HasReminder = hasReminder,
                IsCompleted = false
            };

            MessageBox.Show(
                "✅ Task added successfully!",
                "Task Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            taskService.AddTask(pendingTask);

            ActivityLogService.Add(
                "TASK",
                $"{MemoryStore.UserName} added task '{title}'");

            AppendBotMessage(
                $"✅ Task saved successfully!\n\n" +
                $"📋 Title: {title}\n\n" +
                $"📝 Description:\n{description}");
        }

        private void ViewTasksOption_Click(object sender, RoutedEventArgs e)
        {
            tasks = taskService.LoadTasks();

            if (!tasks.Any())
            {
                AppendBotMessage(
                    "📋 No tasks available.");
                return;
            }

            string response =
                "📋 TASK LIST\n\n";

            foreach (TaskItem task in tasks)
            {
                response +=
                    $"📌 {task.Title}\n" +
                    $"{task.Description}\n" +
                    $"Status: " +
                    $"{(task.IsCompleted ? "✅ Completed" : "⏳ Pending")}\n";

                if (task.HasReminder)
                {
                    response +=
                        $"Reminder: {task.ReminderDate:d}\n";
                }

                response += "\n";
            }

            AppendBotMessage(response);

            ActivityLogService.Add(
                "TASK",
                $"{MemoryStore.UserName} viewed tasks");
        }

        private void CompleteTaskOption_Click(object sender, RoutedEventArgs e)
        {
            
            currentMode = "COMPLETE";

            AppendBotMessage("Enter the task title you want to complete.");
        }

        private void DeleteTaskOption_Click(object sender, RoutedEventArgs e)
        {
            currentAction = "DELETE";

            AppendBotMessage("Enter the task title you want to delete.");
        }
       private void ViewRemindersTaskOption_Click(
    object sender,
    RoutedEventArgs e)
{
    List<TaskItem> reminders =
        taskService.GetDueReminders();

    if (!reminders.Any())
    {
        AppendBotMessage(
            "🔔 No reminders available.");
        return;
    }

    string message =
        "🔔 CURRENT REMINDERS\n\n";

    foreach (TaskItem task in reminders)
    {
        message +=
            $"📌 {task.Title}\n";

        if (task.ReminderDate.HasValue)
        {
            message +=
                $"Reminder Date: " +
                $"{task.ReminderDate.Value:d}\n";
        }

        message += "\n";
    }

    AppendBotMessage(message);

    ActivityLogService.Add(
        "TASK",
        $"{MemoryStore.UserName} viewed reminders");
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

            // User chooses reminder
            if (waitingForReminderDate)
            {
                waitingForReminderDate = false;

                int days = 0;

                if (message.Contains("day"))
                {
                    string number =
                        new string(message.Where(char.IsDigit).ToArray());

                    int.TryParse(number, out days);

                    pendingTask.ReminderDate =
                        DateTime.Now.AddDays(days);
                }
                else
                {
                    pendingTask.ReminderDate =
                        DateTime.Now.AddDays(1);
                }

                taskService.AddTask(pendingTask);

                ActivityLogService.Add(
                    "TASK",
                    $"Reminder set for '{pendingTask.Title}' on {pendingTask.ReminderDate:d}");

                MessageBox.Show(
                    $"Task '{pendingTask.Title}' added.\n\nReminder set for {pendingTask.ReminderDate:d}",
                    "Task Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                AppendBotMessage($"Got it! I'll remind you on {pendingTask.ReminderDate:d}");

                ViewJsonTasksButton.Visibility =
                    Visibility.Visible;

                return;
            }

            if (currentMode == "COMPLETE")
            {
                bool completed =
                    taskService.MarkTaskCompleted(message);

                if (completed)
                {
                    AppendBotMessage($"✅ Task '{message}' marked as completed.");

                    ActivityLogService.Add(
                        "TASK",
                        $"Completed task '{message}'");
                }
                else
                {
                    AppendBotMessage($"❌ Task '{message}' not found.");
                }

                currentMode = "";

                return;
            }

            if (currentAction == "DELETE")
            {
                bool deleted =
                    taskService.DeleteTask(message);

                if (deleted)
                {
                    AppendBotMessage($"🗑 Task '{message}' deleted.");

                    ActivityLogService.Add(
                        "TASK",
                        $"Deleted task '{message}'");
                }
                else
                {
                    AppendBotMessage(
                        $"❌ Task '{message}' not found.");
                }

                currentAction = "";

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

        private void ViewJsonTasksButton_Click(object sender, RoutedEventArgs e)
        {
            string path =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Data",
                    "tasks.json");

            if (!File.Exists(path))
            {
                AppendBotMessage(
                    "❌ No saved task file found.");

                return;
            }

            string json =
                File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json)
                || json == "[]")
            {
                AppendBotMessage(
                    "📁 No tasks have been saved yet.");

                return;
            }

            AppendBotMessage(
                "📁 SAVED JSON TASK FILE\n\n" +
                json);

            ActivityLogService.Add(
                "TASK",
                $"{MemoryStore.UserName} viewed saved JSON file");
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
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} returned to chat");

            if (chatWindow != null)
            {
                chatWindow.Show();
            }

            Close();
        }
    }

}
