using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Services;
using CybersecurityAwarenessChatbot.Models;
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

        // Sends user message to chatbot.
        // Sends user message to chatbot.
        private void SendMessage()
        {
            string input = (UserInputTextBox.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            // Play Notification sound
            VoicePlayer.PlaySound();

            // Display user message
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = char.ToUpper(input[0]) + input.Substring(1);
            }

            AppendUserMessage(input);

            // =====================================================
            // PART 3 NLP DETECTION
            // =====================================================

            string response = "";

            string intent = nlpService.DetectIntent(input);

            switch (intent)
            {
                case "ADD_TASK":

                    TaskItem task = new TaskItem
                    {
                        Title = input,
                        Description = "Task created from chatbot request",
                        ReminderDate = DateTime.Now.AddDays(7),
                        IsCompleted = false
                    };

                    taskService.AddTask(task);

                    logService.AddLog($"Task Added: {task.Title}");

                    response =
                        $"✅ Task added successfully.\n\n" +
                        $"Title: {task.Title}\n" +
                        $"Reminder: {task.ReminderDate:d}";

                    break;

                case "REMINDER":

                    logService.AddLog("Reminder Created");

                    response =
                        "⏰ Reminder recorded.\n\n" +
                        "Don't forget to complete your cybersecurity task.";

                    break;

                case "QUIZ":

                    quizService.CurrentQuestionIndex = 0;
                    quizService.Score = 0;

                    QuizQuestion question =
                        quizService.Questions[0];

                    logService.AddLog("Quiz Started");

                    response =
                        $"🎮 Cybersecurity Quiz Started!\n\n" +
                        $"{question.Question}\n\n";

                    for (int i = 0; i < question.Options.Count; i++)
                    {
                        response += $"{i + 1}. {question.Options[i]}\n";
                    }

                    break;

                case "LOG":

                    List<ActivityLogItem> logs =
                        logService.GetRecentLogs();

                    if (logs.Count == 0)
                    {
                        response = "📊 No activity recorded yet.";
                    }
                    else
                    {
                        response = "📊 Recent Activity:\n\n";

                        foreach (ActivityLogItem log in logs)
                        {
                            response += log + "\n";
                        }
                    }

                    break;

                default:

                    // Existing Part 2 chatbot logic
                    response =
                        _chatBot.ProcessInput(input) ?? string.Empty;

                    break;
            }

            // =====================================================
            // EXISTING CHAT CLEAR FEATURE
            // =====================================================

            if (response.StartsWith("__CLEAR_CHAT__"))
            {
                ChatPanel.Children.Clear();

                response =
                    response.Replace("__CLEAR_CHAT__", "")
                            .Trim();
            }

            // Display bot response
            AppendBotMessage(response);

            // Clear input
            UserInputTextBox.Clear();

            // Scroll chat
            ChatScrollViewer.ScrollToBottom();
        }

        private void CheckQuizAnswer(string input)
        {
            if (quizService.CurrentQuestionIndex >= quizService.Questions.Count)
                return;

            QuizQuestion currentQuestion =
                quizService.Questions[quizService.CurrentQuestionIndex];

            if (int.TryParse(input, out int answer))
            {
                answer--;

                if (answer == currentQuestion.CorrectAnswer)
                {
                    quizService.Score++;

                    AppendBotMessage(
                        $"✅ Correct!\n\n{currentQuestion.Explanation}");
                }
                else
                {
                    AppendBotMessage(
                        $"❌ Incorrect.\n\n{currentQuestion.Explanation}");
                }

                quizService.CurrentQuestionIndex++;

                if (quizService.CurrentQuestionIndex <
                    quizService.Questions.Count)
                {
                    QuizQuestion next =
                        quizService.Questions[quizService.CurrentQuestionIndex];

                    AppendBotMessage(
                        $"Question {quizService.CurrentQuestionIndex + 1}\n\n" +
                        next.Question +
                        "\n\n1. " +
                        string.Join("\n", next.Options.Select((x, i) =>
                            $"{i + 1}. {x}")));
                }
                else
                {
                    AppendBotMessage(
                        $"🏆 Quiz Finished!\n\n" +
                        $"Score: {quizService.Score}/{quizService.Questions.Count}");

                    logService.AddLog(
                        $"Quiz Completed - Score {quizService.Score}/{quizService.Questions.Count}");
                }
            }
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