using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;
using CybersecurityAwarenessChatbot.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityAwarenessChatbot
{
    // Main GUI window for SecureWin chatbot.
    // Handles user interaction and displays chat messages.
    public partial class ChatWindow : Window
    {
        private ChatBot _chatBot;

        private readonly MemoryStore memoryStore = new()    ;

        // tracking list of answers

        private List<int> wrongQuestionIndexes = new();

        // Menu state tracking
        private bool isInMenuMode = true;

        // Stores selected task for delete/complete operations
        private int selectedTaskId = -1;

        private TaskItem pendingTask = null;
        private bool awaitingReminder = false;

        // Tracks quiz state
        private bool quizInProgress = false;

        private readonly TaskService taskService = new();
        private readonly QuizService quizService = new();
        private readonly ActivityLogService logService = new();
        private readonly NLPService nlpService = new();

        public ChatWindow()
        {
            InitializeComponent();

            // Initialize chatbot
            _chatBot = new ChatBot();

            // Disable textbox and buttons during startup greeting
            UserInputTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            TaskButton.IsEnabled = false;
            QuizButton.IsEnabled = false;
            ActivityButton.IsEnabled = false;

            // Load ASCII art
            AsciiArtText.Text = UIHelper.ShowLogo();

            // Run startup sequence
            Loaded += MainWindow_Loaded;
        }

        // SHOW MAIN MENU (NUMBERED)
        private string ShowMainMenu()
        {
            return
                "🔐 SECUREWIN MAIN MENU\n\n" +
                "1. 💬 Chat\n" +
                "2. 🎮 Quiz\n" +
                "3. 📋 Tasks\n" +
                "4. 📊 Activity Log\n" +
                "5. ❌ End Session\n\n" +
                "Type a number to continue.";
        }

        private bool _isGreetingPlayed = false;
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // Check if it already ran
            if (_isGreetingPlayed) return;
            _isGreetingPlayed = true;

            // Play greeting voice
            await Task.Run(() => VoicePlayer.PlayGreeting());

            // Play a quick system notification ping
            System.Media.SystemSounds.Asterisk.Play();

            // Show chatbot greeting after audio
            //AppendBotMessage(_chatBot.GetGreeting());

            // ⏳ Small delay for UX
            await Task.Delay(800);
           
            AppendBotMessage(ShowMainMenu());

            isInMenuMode = true;

            // Enable user interaction
            UserInputTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            TaskButton.IsEnabled = true;
            QuizButton.IsEnabled = true;
            ActivityButton.IsEnabled = true;

            // Focus textbox automatically
            UserInputTextBox.Focus();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTasks();
        }

        private void ViewTasks_Click(object sender, RoutedEventArgs e)
        {
            List<TaskItem> tasks =
                taskService.GetTasks();

            if (tasks.Count == 0)
            {
                AppendBotMessage("No tasks found.");
                return;
            }

            string output = "";

            foreach (TaskItem task in tasks)
            {
                output += task + "\n\n";
            }

            AppendBotMessage(output);
        }

        // DELETE TASK
        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTaskId == -1)
            {
                AppendBotMessage(
                    "⚠ Please select or view a task first.");
                return;
            }

            taskService.DeleteTask(selectedTaskId);

            logService.AddLog(
                $"Deleted Task {selectedTaskId}");

            AppendBotMessage(
                $"🗑 Task {selectedTaskId} has been deleted successfully.");

            selectedTaskId = -1;
        }

        // COMPLETE TASK
        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTaskId == -1)
            {
                AppendBotMessage(
                    "⚠ Please select or view a task first.");
                return;
            }

            taskService.CompleteTask(selectedTaskId);

            logService.AddLog(
                $"Completed Task {selectedTaskId}");

            AppendBotMessage(
                $"✅ Task {selectedTaskId} marked as completed.");

            selectedTaskId = -1;
        }

        // SHOW ALL TASKS IN CHAT
        private void ShowTasks()
        {
            List<TaskItem> tasks = taskService.GetTasks();

            if (tasks.Count == 0)
            {
                AppendBotMessage("📋 No cybersecurity tasks found.");
                return;
            }

            AppendBotMessage("📋 TASK LIST LOADED:");

            foreach (TaskItem task in tasks)
            {
                AppendTaskMessage(task); // 👈 clickable UI per task
            }
        }

        // Handles send button click.
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Allows Enter key to send messages.
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
            }
        }

        // CHECK QUIZ ANSWER
        private void CheckQuizAnswer(string input)
        {
            if (quizService.CurrentQuestionIndex >= quizService.Questions.Count)
                return;

            QuizQuestion question =
                quizService.Questions[quizService.CurrentQuestionIndex];

            if (int.TryParse(input, out int answer))
            {
                answer--; // convert to 0-based index

                if (answer == question.CorrectAnswer)
                {
                    quizService.Score++;

                    AppendBotMessage(
                        $"✅ Correct!\n\n{question.Explanation}");
                }
                else
                {
                    AppendBotMessage(
                        $"❌ Incorrect.\n\n{question.Explanation}");

                    wrongQuestionIndexes.Add(quizService.CurrentQuestionIndex);
                }

                quizService.CurrentQuestionIndex++;

                // NEXT QUESTION
                if (quizService.CurrentQuestionIndex < quizService.Questions.Count)
                {
                    QuizQuestion next =
                        quizService.Questions[quizService.CurrentQuestionIndex];

                    string response =
                        $"📘 Question {quizService.CurrentQuestionIndex + 1}\n\n" +
                        next.Question + "\n\n";

                    for (int i = 0; i < next.Options.Count; i++)
                    {
                        response += $"{i + 1}. {next.Options[i]}\n";
                    }

                    AppendBotMessage(response);
                }
                else
                {
                    ShowQuizResults();
                }
            }
        }

        // GET QUIZ BADGE BASED ON SCORE
        private string GetQuizBadge(int score, int total)
        {
            double percentage = (double)score / total * 100;

            if (percentage == 100)
                return "🏆 PERFECT - Cybersecurity Expert";
            else if (percentage >= 70)
                return "🥈 GOOD - Strong Awareness";
            else
                return "🥉 TRY AGAIN - Needs Improvement";
        }

        // Sends user message to chatbot.
        // Sends user message to chatbot.
        private void SendMessage()
        {
            string input = (UserInputTextBox.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            VoicePlayer.PlaySound();

            AppendUserMessage(input);
            UserInputTextBox.Clear();

            string response = "";

            // =========================
            // MENU HANDLING FIRST
            // =========================
            if (isInMenuMode)
            {
                switch (input)
                {
                    case "1":
                        isInMenuMode = false;
                        response = "💬 Chat mode activated.\n\nAsk me anything about cybersecurity.";
                        break;

                    case "2":
                        isInMenuMode = false;
                        QuizButton_Click(null, null);
                        return;

                    case "3":
                        isInMenuMode = false;
                        ShowTasks();
                        response = "📋 Task menu opened.";
                        break;

                    case "4":
                        isInMenuMode = false;
                        ActivityButton_Click(null, null);
                        response = "📊 Activity log displayed.";
                        break;

                    case "5":
                        Application.Current.Shutdown();
                        return;

                    default:
                        response = "⚠ Please select a valid option (1–5).";
                        break;
                }

                AppendBotMessage(response);
                AppendBotMessage(ShowMainMenu());
                return;
            }

            // =========================
            // QUIZ MODE
            // =========================
            if (quizInProgress)
            {
                CheckQuizAnswer(input);
                return;
            }

            // =========================
            // NLP TASK DETECTION
            // =========================
            string intent = nlpService.DetectTaskIntent(input);

            switch (intent)
            {
                case "ADD_TASK":

                    TaskItem task = new TaskItem
                    {
                        Title = input,
                        Description = "Cybersecurity task created via chatbot",
                        ReminderDate = DateTime.Now.AddDays(7),
                        IsCompleted = false
                    };

                    taskService.AddTask(task);
                    logService.AddLog($"Task Added: {task.Title}");

                    response =
                        $"✅ Task Added\n\nTitle: {task.Title}\nReminder: {task.ReminderDate:d}";
                    break;

                case "REMINDER":
                    logService.AddLog("Reminder Created");
                    response = "⏰ Reminder saved successfully.";
                    break;

                case "QUIZ":
                    QuizButton_Click(null, null);
                    return;

                case "LOG":
                    ActivityButton_Click(null, null);
                    return;

                default:
                    response = _chatBot.ProcessInput(input) ?? "";
                    break;
            }

            AppendBotMessage(response);

            // always show menu again after action
            AppendBotMessage(ShowMainMenu());
        }

        // SHOW FINAL QUIZ RESULTS
        private void ShowQuizResults()
        {
            QuizQuestion question = quizService.Questions[quizService.CurrentQuestionIndex];

            int total = quizService.Questions.Count;

            int score = quizService.Score;

            string badge = GetQuizBadge(score, total);

            string result =
                "🏆 QUIZ COMPLETED\n\n" +
                $"Score: {score}/{total}\n" +
                $"Badge: {badge}\n\n";

            for (int i = 0; i < quizService.Questions.Count; i++)
            {
                QuizQuestion q = quizService.Questions[i];

                result +=
                    $"Q{i + 1}: {q.Question}\n" +
                    $"✔ Correct Answer: {q.Options[q.CorrectAnswer]}\n\n";
            }

            if (score < total)
            {
                result += "🔁 Type 'retry wrong' to retry only incorrect answers.";
            }

            else
            {
                AppendBotMessage(
                    $"❌ Incorrect.\n\n{question.Explanation}");

                wrongQuestionIndexes.Add(quizService.CurrentQuestionIndex);
            }

            AppendBotMessage(result);
            quizInProgress = false;
        }

        // DISPLAY TASK WITH CLICKABLE ID
        private void AppendTaskMessage(TaskItem task)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 38, 58)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 750
            };

            StackPanel panel = new StackPanel();

            // CLICKABLE TASK ID
            Button idButton = new Button
            {
                Content = $"🆔 Task ID: {task.Id}",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5),
                Padding = new Thickness(5),
                Tag = task.Id
            };

            idButton.Click += TaskId_Click;

            TextBlock text = new TextBlock
            {
                Text =
                    $"📌 Title: {task.Title}\n" +
                    $"📝 Description: {task.Description}\n" +
                    $"⏰ Reminder: {task.ReminderDate:d}\n" +
                    $"✅ Completed: {(task.IsCompleted ? "Yes" : "No")}",
                Foreground = Brushes.White,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap
            };

            panel.Children.Add(idButton);
            panel.Children.Add(text);

            bubble.Child = panel;

            ChatPanel.Children.Add(bubble);
        }

        // TASK ID CLICK HANDLER

        private void TaskId_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int taskId)
            {
                selectedTaskId = taskId;

                AppendBotMessage($"🆔 Task {taskId} selected.\nYou can now delete or complete it.");
            }
        }

        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            quizService.CurrentQuestionIndex = 0;
            quizService.Score = 0;

            quizInProgress = true;

            QuizQuestion question =
                quizService.Questions[0];

            string response =
                "🎮 CYBERSECURITY QUIZ STARTED\n\n" +
                question.Question + "\n\n";

            for (int i = 0; i < question.Options.Count; i++)
            {
                response += $"{i + 1}. {question.Options[i]}\n";
            }

            logService.AddLog("Quiz Started");

            AppendBotMessage(response);
        }

        private void ActivityButton_Click(object sender, RoutedEventArgs e)
        {
            List<ActivityLogItem> logs =
                logService.GetRecentLogs();

            if (logs.Count == 0)
            {
                AppendBotMessage(
                    "📊 No activity recorded yet.");
                return;
            }

            string output =
                "📊 ACTIVITY LOG\n\n";

            foreach (ActivityLogItem log in logs)
            {
                output +=
                    $"{log.TimeStamp:HH:mm} - {log.Action}\n";
            }

            AppendBotMessage(output);
        }


        // Handles placeholder text behavior.
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility =
                string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        // Displays user message bubble.
        private void AppendUserMessage(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0, 194, 255)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                MaxWidth = 700
            };

            TextBlock text = new TextBlock
            {
                Text = $"🧑 You [{DateTime.Now:HH:mm}]\n\n{message}",
                Foreground = Brushes.White,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            };

            bubble.Child = text;

            ChatPanel.Children.Add(bubble);
        }

        // Displays bot message bubble.
        private void AppendBotMessage(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 38, 58)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 750
            };

            TextBlock text = new TextBlock
            {
                Text = $"🤖 SecureWin [{DateTime.Now:HH:mm}]\n\n{message}",
                Foreground = Brushes.White,
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap
            };

            bubble.Child = text;

            ChatPanel.Children.Add(bubble);
        }

    }
}