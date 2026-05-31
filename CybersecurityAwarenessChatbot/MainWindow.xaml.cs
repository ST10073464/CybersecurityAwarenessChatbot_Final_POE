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
    public partial class MainWindow : Window
    {
        private ChatBot _chatBot;

        // Stores selected task for delete/complete operations
        private int selectedTaskId = -1;

        // Tracks quiz state
        private bool quizInProgress = false;

        private readonly TaskService taskService = new();
        private readonly QuizService quizService = new();
        private readonly ActivityLogService logService = new();
        private readonly NLPService nlpService = new();

        public MainWindow()
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
            AppendBotMessage(_chatBot.GetGreeting());

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

        // =====================================================
        // DELETE TASK
        // =====================================================
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

        // =====================================================
        // COMPLETE TASK
        // =====================================================
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

        // =====================================================
        // SHOW ALL TASKS IN CHAT
        // =====================================================
        private void ShowTasks()
        {
            List<TaskItem> tasks =
                taskService.GetTasks();

            if (tasks.Count == 0)
            {
                AppendBotMessage(
                    "📋 No cybersecurity tasks found.");
                return;
            }

            string message =
                "📋 CYBERSECURITY TASKS\n\n";

            foreach (TaskItem task in tasks)
            {
                message +=
                    $"ID: {task.Id}\n" +
                    $"Title: {task.Title}\n" +
                    $"Description: {task.Description}\n" +
                    $"Reminder: {task.ReminderDate:d}\n" +
                    $"Completed: {(task.IsCompleted ? "Yes" : "No")}\n\n";
            }

            AppendBotMessage(message);
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

        // =====================================================
        // CHECK QUIZ ANSWER
        // =====================================================

        private void CheckQuizAnswer(string input)
        {
            if (!int.TryParse(input, out int answer))
            {
                AppendBotMessage(
                    "⚠ Please enter a number.");
                return;
            }

            QuizQuestion currentQuestion =
                quizService.Questions[
                    quizService.CurrentQuestionIndex];

            answer--;

            if (answer == currentQuestion.CorrectAnswer)
            {
                quizService.Score++;

                AppendBotMessage(
                    "✅ Correct!\n\n" +
                    currentQuestion.Explanation);
            }
            else
            {
                AppendBotMessage(
                    "❌ Incorrect.\n\n" +
                    currentQuestion.Explanation);
            }

            quizService.CurrentQuestionIndex++;

            // More questions available
            if (quizService.CurrentQuestionIndex <
                quizService.Questions.Count)
            {
                QuizQuestion nextQuestion =
                    quizService.Questions[
                        quizService.CurrentQuestionIndex];

                string message =
                    $"Question {quizService.CurrentQuestionIndex + 1}\n\n" +
                    $"{nextQuestion.Question}\n\n";

                for (int i = 0;
                     i < nextQuestion.Options.Count;
                     i++)
                {
                    message +=
                        $"{i + 1}. {nextQuestion.Options[i]}\n";
                }

                AppendBotMessage(message);
            }
            else
            {
                quizInProgress = false;

                AppendBotMessage(
                    $"🏆 Quiz Complete!\n\n" +
                    $"Final Score: " +
                    $"{quizService.Score}/" +
                    $"{quizService.Questions.Count}");

                logService.AddLog(
                    $"Quiz Completed - Score " +
                    $"{quizService.Score}/" +
                    $"{quizService.Questions.Count}");
            }
        }

        // Sends user message to chatbot.
        // Sends user message to chatbot.
        private void SendMessage()
        {
            string input =
                (UserInputTextBox.Text ?? "")
                .Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            VoicePlayer.PlaySound();

            AppendUserMessage(input);

            // ===================================
            // QUIZ ANSWER MODE
            // ===================================

            if (quizInProgress)
            {
                CheckQuizAnswer(input);

                UserInputTextBox.Clear();

                ChatScrollViewer.ScrollToBottom();

                return;
            }

            string response = "";

            string intent =
                nlpService.DetectIntent(input);

            switch (intent)
            {
                case "ADD_TASK":

                    TaskItem task = new TaskItem
                    {
                        Title = input,
                        Description =
                            "Cybersecurity task created via chatbot",
                        ReminderDate =
                            DateTime.Now.AddDays(7),
                        IsCompleted = false
                    };

                    taskService.AddTask(task);

                    logService.AddLog(
                        $"Task Added: {task.Title}");

                    response =
                        $"✅ Task Added\n\n" +
                        $"Title: {task.Title}\n" +
                        $"Reminder Date: {task.ReminderDate:d}";

                    break;

                case "REMINDER":

                    logService.AddLog(
                        "Reminder Created");

                    response =
                        "⏰ Reminder created successfully.";

                    break;

                case "QUIZ":

                    QuizButton_Click(null, null);

                    UserInputTextBox.Clear();

                    return;

                case "LOG":

                    ActivityButton_Click(null, null);

                    UserInputTextBox.Clear();

                    return;

                default:

                    response =
                        _chatBot.ProcessInput(input)
                        ?? "";

                    break;
            }

            if (response.StartsWith("__CLEAR_CHAT__"))
            {
                ChatPanel.Children.Clear();

                response =
                    response.Replace(
                        "__CLEAR_CHAT__",
                        "")
                    .Trim();
            }

            AppendBotMessage(response);

            UserInputTextBox.Clear();

            ChatScrollViewer.ScrollToBottom();
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