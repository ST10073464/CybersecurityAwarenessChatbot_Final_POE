/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;

namespace CybersecurityAwarenessChatbot
{
    // Main GUI window for SecureWin chatbot.
    // Handles user interaction and displays chat messages.
    public partial class MainWindow : Window
    {
        private readonly ChatBot _chatBot;

        private bool _isGreetingPlayed = false;

        private bool awaitingUserName = true;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Show username in header
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Initialize chatbot
            _chatBot = new ChatBot();

            // Disable textbox during startup greeting
            UserInputTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            // load ASCII art logo
            AsciiArtText.Text = UIHelper.ShowLogo();

            // Run startup sequence
            Loaded += MainWindow_Loaded;

            // Log user activity
            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} started chatbot");
        }

        // On window load, display welcome message and activity log summary.
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

            if (string.IsNullOrWhiteSpace(MemoryStore.UserName))
            {
                MemoryStore.UserName = "Guest";
            }

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // load recent activity log
            LoadRecentActivity();

            // Focus textbox automatically
            UserInputTextBox.Focus();
        }

        // Send button click event handler
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Allows Enter key to send messages.
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        // load recent activity log into the ActivityLogText TextBox
        private void LoadRecentActivity()
        {
            ActivityLogText.Text = ActivityLogService.GetAllLogs();
        }      

        // Send user message when Send button is clicked.
        private void SendMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            // Play Notification sound
            VoicePlayer.PlaySound();

            // Display user message
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = char.ToUpper(input[0]) + input.Substring(1);
            }

            // Display user message
            AppendUserMessage(input);

            string response = _chatBot.ProcessInput(input); //string.Empty;

            if (awaitingUserName)
            {
                awaitingUserName = false;

                UserNameText.Text = $"👤 {MemoryStore.UserName}";
            }

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Log user activity
            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} sent message: '{input}'");

            // CLEAR CHAT if signal is found
            if (response.StartsWith("__CLEAR_CHAT__"))
            {
                ChatPanel.Children.Clear();

                response = response.Replace("__CLEAR_CHAT__", "").Trim();
            }

            switch (response)
            {
                case "__OPEN_TASK_WINDOW__":

                    UserInputTextBox.Clear();

                    TaskWindow taskWindow = new TaskWindow("", this);
                    taskWindow.Show();
                    Hide();
                    return;

                case "__OPEN_QUIZ__":

                    UserInputTextBox.Clear();

                    new QuizWindow(this).Show();

                    this.Hide();

                    return;

                case "__SHOW_ACTIVITY_LOG__":

                    AppendBotMessage(ActivityLogService.GetAllLogs());

                    break;

                case "__OPEN_TASK_REMINDERS__":

                    new TaskWindow("REMINDERS").Show();

                    Close();

                    return;

                case "__LEAVE_SESSION__":

                    // Save username BEFORE anything resets it
                    string currentUser = UserNameText.Text.Replace("👤", "").Trim();

                    MessageBox.Show(
                        $"{currentUser} ended the chat.",
                        "Session Ended",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Log activity before resetting
                    ActivityLogService.Add("MAIN", $"{currentUser} ended the session");

                    // Reset chatbot state
                    _chatBot.ResetSession();

                    // Reset username
                    MemoryStore.UserName = "Guest";

                    // Update GUI immediately
                    UserNameText.Text = "👤 Guest";

                    // Clear chat window
                    ChatPanel.Children.Clear();

                    // Show welcome message for a new user
                    AppendBotMessage(
                        "👋 Welcome to SecureWin!\n\n" +
                        "Please type your name to continue.");

                    UserInputTextBox.Focus();

                    UserInputTextBox.Clear();

                    return;

                case "__CLOSE_SECUREWIN__":

                    MessageBoxResult result =
                        MessageBox.Show(
                            "Are you sure you want to close SecureWin?",
                            "Close SecureWin",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        MemoryStore.UserName = "";

                        System.Windows.Application.Current.Shutdown();
                    }

                    return;

            }

            // Display bot response
            AppendBotMessage(response);

            // Clear textbox immediately
            UserInputTextBox.Clear();

            // Scroll to latest message
            Dispatcher.InvokeAsync(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            });

            // Return focus to textbox
            UserInputTextBox.Focus();
        }

        // Placeholder text visibility based on user input
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
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

            Dispatcher.InvokeAsync(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            });
        }

        // Displays bot message bubble.
        private void AppendBotMessage(string message)
        {
            StackPanel container = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5)
            };

            TextBlock header = new TextBlock
            {
                Text = $"🤖 Cybersecurity Assistant • {DateTime.Now:HH:mm}",
                Foreground = Brushes.Gray,
                FontSize = 13,
                FontWeight = FontWeights.Bold
            };

            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 38, 58)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                MaxWidth = 750
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

            Dispatcher.InvokeAsync(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            });
        }

        private void ShowSavedJsonTask()
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Data",
                "tasks.json");

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

            AppendBotMessage(
                "📁 SAVED TASKS (JSON)\n\n" +
                json);
        }

        private void ViewJsonTasksButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSavedJsonTask();
        }

        private void ViewLogsButton_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage("📋 RECENT ACTIVITY\n\n" + ActivityLogService.GetAllLogs());
        }
    }
}