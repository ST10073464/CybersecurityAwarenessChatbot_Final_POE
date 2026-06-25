/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using CybersecurityAwarenessChatbot.Services;
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

        // Routing States

        // Tracks "COMPLETE" or "DELETE"
        private string currentAction = ""; 
        private string pendingTitle = "";
        private string pendingDescription = "";

        private bool waitingForDescription = false;
        private bool waitingForReminderChoice = false;
        private bool waitingForReminderDate = false;

        private TaskItem pendingTask;

        public TaskWindow(string mode = "", MainWindow mainWindow = null)
        {
            InitializeComponent();

            chatWindow = mainWindow;
            taskService = new TaskService();
            tasks = taskService.LoadTasks();

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            //Loaded += TaskWindow_Loaded;
            Closing += Window_Closing;

            AppendBotMessage(MemoryStore.TaskWelcomeMessage);
            
            LoadActivityLog();
        }


        private void TaskWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UserNameText.Text = $"👤 {MemoryStore.UserName}";
            UserInputTextBox.Focus();

            // This is the ONE true place that handles the window opening log
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened Task Window");

            // Refresh UI textbox
            LoadActivityLog(); 
        }

        private void LoadActivityLog()
        {
            // Only shows logs labeled with the [TASK] tag
            ActivityLogTextBox.Text = ActivityLogService.GetLogsByCategory("TASK");
        }

        // --- LEFT MENU EVENT HANDLERS ---

        private void AddTaskOption_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerAddTaskAction();
        }

        private void ViewTasksOption_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerViewTasksAction();
        }

        private void CompleteTaskOption_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerCompleteTaskAction();
        }

        private void DeleteTaskOption_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerDeleteTaskAction();
        }

        private void ViewRemindersTaskOption_Click(object sender, MouseButtonEventArgs e)
        {
            TriggerViewRemindersAction();
        }

        // --- CENTRALIZED COMMAND STATE ACTIONS (SHARED BY CLICK & TEXT INTERFACES) ---

        private void TriggerAddTaskAction()
        {
            currentAction = "";
            ResetCreationStates();
            AppendBotMessage("✨ Let's add a new task! What is the **Title** of your task?");
        }

        private void TriggerViewTasksAction()
        {
            currentAction = "";
            ResetCreationStates();
            DisplayAllTasks();
        }

        private void TriggerCompleteTaskAction()
        {
            ResetCreationStates();
            currentAction = "COMPLETE";
            AppendBotMessage("✅ Please type the exact **Title** of the task you want to complete:");
        }

        private void TriggerDeleteTaskAction()
        {
            ResetCreationStates();
            currentAction = "DELETE";
            AppendBotMessage("🗑 Please type the exact **Title** of the task you want to delete:");
        }

        private void TriggerViewRemindersAction()
        {
            currentAction = "";
            ResetCreationStates();
            DisplayReminders();
        }

        private void TriggerViewJsonAction()
        {
            currentAction = "";
            ResetCreationStates();
            LoadAndDisplayJsonTasks();
        }

        // --- CORE CHAT PROCESSING ENGINE ---

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessUserCommand();
        }

        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserCommand();
                e.Handled = true;
            }
        }

        private void ProcessUserCommand()
        {
            string userMessage = UserInputTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            AppendUserMessage(userMessage);
            UserInputTextBox.Clear();

            SendTaskMessage(userMessage);
            LoadActivityLog();
        }

        private void SendTaskMessage(string message)
        {
            string cleanInput = message.ToLower();

            // STEP 1: INTERCEPT EXPLICIT COMMAND KEYWORDS REGARDLESS OF ACTIVE CONVERSATION CONTEXT FLAG
            if (cleanInput == "add" || cleanInput == "add task")
            {
                TriggerAddTaskAction();
                return;
            }
            if (cleanInput == "view" || cleanInput == "view tasks")
            {
                TriggerViewTasksAction();
                return;
            }
            if (cleanInput == "complete" || cleanInput == "complete task")
            {
                TriggerCompleteTaskAction();
                return;
            }
            if (cleanInput == "delete" || cleanInput == "delete task")
            {
                TriggerDeleteTaskAction();
                return;
            }
            if (cleanInput == "view reminders" || cleanInput == "reminders")
            {
                TriggerViewRemindersAction();
                return;
            }
            if (cleanInput == "view saved json tasks" || cleanInput == "json")
            {
                TriggerViewJsonAction();
                return;
            }
            if (cleanInput == "back" || cleanInput == "exit")
            {
                ReturnToMainMenu();
                return;
            }

            // STEP 2: PARSE ACTION STAGE CONTEXT ROUTINES IF USER IS CURRENTLY TARGETING INTERACTIVE FLOWS
            if (currentAction == "COMPLETE")
            {
                bool completed = taskService.MarkTaskCompleted(message);
                if (completed)
                {
                    MessageBox.Show($"✅ Task '{message}' marked as completed successfully!", "Task Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppendBotMessage($"✅ Task **'{message}'** has been successfully marked as completed.");
                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} completed task '{message}'");
                }
                else
                {
                    AppendBotMessage($"❌ System error: A task titled '{message}' could not be located.");
                }

                currentAction = "";
                return;
            }

            if (currentAction == "DELETE")
            {
                bool deleted = taskService.DeleteTask(message);
                if (deleted)
                {
                    MessageBox.Show($"🗑 Task '{message}' has been permanently dropped.", "Task Deleted", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AppendBotMessage($"🗑 Task **'{message}'** was permanently removed from your registers.");
                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} deleted task '{message}'");
                }
                else
                {
                    AppendBotMessage($"❌ System error: A task titled '{message}' could not be located.");
                }

                currentAction = "";
                return;
            }

            // STEP 3: RUN CONVERSATIONAL MULTI-STEP CREATION ENGINE PIPELINE
            if (!waitingForDescription && !waitingForReminderChoice && !waitingForReminderDate)
            {
                pendingTitle = message;
                waitingForDescription = true;
                AppendBotMessage($"Title saved: **{pendingTitle}**\n\nWhat is the description or details for this task?");
                return;
            }

            if (waitingForDescription)
            {
                pendingDescription = message;
                waitingForDescription = false;
                waitingForReminderChoice = true;
                AppendBotMessage($"Description noted.\n\nWould you like to associate a reminder time alert for this task? (**Yes**/**No**)");
                return;
            }

            if (waitingForReminderChoice)
            {
                waitingForReminderChoice = false;
                if (message.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    waitingForReminderDate = true;
                    AppendBotMessage("In how many days from today should this reminder activate? (Enter a plain number like *3* or *7*)");
                }
                else
                {
                    SaveTaskObject(false, 0);
                }
                return;
            }

            if (waitingForReminderDate)
            {
                waitingForReminderDate = false;
                string cleanDigits = new string(message.Where(char.IsDigit).ToArray());

                if (int.TryParse(cleanDigits, out int daysCount))
                {
                    SaveTaskObject(true, daysCount);
                }
                else
                {
                    AppendBotMessage("⚠️ Invalid numeric timeframe entry. Defaulting task to a 1-day reminder.");
                    SaveTaskObject(true, 1);
                }
                return;
            }
        }

        // --- REUSABLE UTILITY SUBROUTINES ---

        private void SaveTaskObject(bool setReminder, int finalDays)
        {
            DateTime alertTime = setReminder ? DateTime.Today.AddDays(finalDays) : DateTime.MinValue;

            pendingTask = new TaskItem
            {
                Title = pendingTitle,
                Description = pendingDescription,
                ReminderDate = alertTime,
                HasReminder = setReminder,
                IsCompleted = false
            };

            taskService.AddTask(pendingTask);
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} added task '{pendingTitle}'");

            MessageBox.Show("✅ Task added successfully!", "Task Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            AppendBotMessage(
                $"✅ **Task Successfully Formed & Compiled!**\n\n" +
                $"📌 *Title:* {pendingTitle}\n" +
                $"📝 *Details:* {pendingDescription}\n" +
                (setReminder ? $"🔔 *Alert Date:* {alertTime:yyyy-MM-dd} (In {finalDays} days)" : "🔕 *Alerts:* None configured"));
        }

        private void DisplayAllTasks()
        {
            tasks = taskService.LoadTasks();

            if (tasks == null || !tasks.Any())
            {
                AppendBotMessage("📋 Your operational task register is currently empty.");
                return;
            }

            string responseBuffer = "📋 **CURRENT SYSTEM TASK LIST**\n\n";
            foreach (TaskItem task in tasks)
            {
                responseBuffer += $"📌 **{task.Title}**\n" +
                                  $"   Description: {task.Description}\n" +
                                  $"   Status: {(task.IsCompleted ? "✅ Completed" : "⏳ Pending")}\n";

                if (task.HasReminder && task.ReminderDate.HasValue)
                {
                    responseBuffer += $"   🔔 Alert On: {task.ReminderDate.Value:yyyy-MM-dd}\n";
                }
                responseBuffer += "--------------------------------------\n";
            }

            AppendBotMessage(responseBuffer);
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} viewed all tasks");
        }

        private void DisplayReminders()
        {
            tasks = taskService.LoadTasks();
            var filteredTasks = tasks.Where(t => t.HasReminder).ToList();

            if (!filteredTasks.Any())
            {
                AppendBotMessage("🔔 No configured task alerts or reminders found.");
                return;
            }

            string responseBuffer = "🔔 **ACTIVE REGISTERED REMINDERS**\n\n";
            foreach (TaskItem task in filteredTasks)
            {
                string deadlineStr = task.ReminderDate.HasValue ? task.ReminderDate.Value.ToString("yyyy-MM-dd") : "Not Set";
                responseBuffer += $"📌 **{task.Title}**\n" +
                                  $"   ⏰ Alert Date Target: {deadlineStr}\n" +
                                  $"   State Context: {(task.IsCompleted ? "✅ Completed" : "⏳ Pending")}\n\n";
            }

            AppendBotMessage(responseBuffer);
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} checked explicit task reminders");
        }

        private void LoadAndDisplayJsonTasks()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tasks.json");

            if (!File.Exists(path))
            {
                AppendBotMessage("❌ No saved task file found.");
                return;
            }

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json) || json == "[]")
            {
                AppendBotMessage("📁 No tasks have been saved yet.");
                return;
            }

            AppendBotMessage("📁 SAVED JSON TASK FILE\n\n" + json);
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} viewed saved JSON file");
        }

        private void ResetCreationStates()
        {
            waitingForDescription = false;
            waitingForReminderChoice = false;
            waitingForReminderDate = false;
        }

        private void ReturnToMainMenu()
        {
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} returned to chat");
            if (chatWindow != null) chatWindow.Show();
            Close();
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
            ChatScrollViewer.ScrollToBottom();
        }

        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text) 
                ? Visibility.Visible 
                : Visibility.Hidden;
        }

        private void ViewJsonTasksButton_Click(object sender, RoutedEventArgs e)
        {
            TriggerViewJsonAction();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ActivityLogService.Add("TASK", $"{MemoryStore.UserName} closed Task Window");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ReturnToMainMenu();
        }
    }
}