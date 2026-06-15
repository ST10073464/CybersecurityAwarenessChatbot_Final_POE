/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
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
        private readonly ChatBot _chatBot = new();

        // Constructor
        public ChatWindow()
        {
            InitializeComponent();

            // Show username in header
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Initialize chatbot
            _chatBot = new ChatBot();

            // Load event handler for chat mode startup
            Loaded += ChatWindow_Loaded;

        }

        // Chat mode Startup
        private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Refresh username display.
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Display welcome message with topics.
            AppendBotMessage(
                $"👋 Welcome {MemoryStore.UserName} to the Cybersecurity Chatbot Assistant!\n\n" +
                $"You can ask me anything about cybersecurity.\n\n" +
                $"Topics include:\n\n" +
                $"🔒 Passwords\n" +
                $"🎣 Phishing\n" +
                $"🛡️ Privacy\n" +
                $"💻 Malware\n" +
                $"⚠️ Scams");

            ShowMainMenuButtons();
        }

        // Creates styled menu buttons for main topics.
        private void ShowMainMenuButtons()
        {
            WrapPanel panel = new WrapPanel
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Button quizButton =
                CreateMenuButton("🎮 Quiz Mode");

            Button taskButton =
                CreateMenuButton("📋 Task Mode");

            Button logButton =
                CreateMenuButton("📑 Activity Log");

            quizButton.Click += QuizButton_Click;
            taskButton.Click += TaskButton_Click;
            logButton.Click += ActivityLogButton_Click;

            panel.Children.Add(quizButton);
            panel.Children.Add(taskButton);
            panel.Children.Add(logButton);

            ChatPanel.Children.Add(panel);
        }

        // Event handlers for menu buttons
        private Button CreateMenuButton(string text)
        {
            return new Button
            {
                Content = text,
                Width = 180,
                Height = 45,
                Margin = new Thickness(5),
                Background = Brushes.DeepSkyBlue,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
        }

        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            QuizWindow quizWindow = new QuizWindow();

            quizWindow.Show();

            Close();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            TaskWindow taskWindow = new TaskWindow();

            taskWindow.Show();

            Close();
        }

        private void ActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            ActivityLogService logWindow = new ActivityLogService();

            //logWindow.Show();

            Close();
        }

        // Send user message when Send button is clicked.
        private void SendMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            VoicePlayer.PlaySound();

            AppendUserMessage(input);

            string response = _chatBot.ProcessInput(input);

            if (response.StartsWith("__CLEAR_CHAT__"))
            {
                ChatPanel.Children.Clear();

                response = response.Replace("__CLEAR_CHAT__", "").Trim();
            }

            AppendBotMessage(response);

            UserInputTextBox.Clear();

            ChatScrollViewer.ScrollToBottom();
        }

        // Allows Enter key to send messages.
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
            }
        }

        // Placeholder text visibility based on user input
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        // Send button click event handler
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Back button click event handler - returns to the landing page and clears chat history.
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LandingPage landing = new LandingPage();

            landing.Show();

            Close();
        }

        // Displays user message bubble.
        private void AppendUserMessage(string message)
        {
            StackPanel container = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            TextBlock header = new()
            {
                Text = $"{MemoryStore.UserName} • {DateTime.Now:HH:mm}",
                Foreground = Brushes.LightGray,
                FontWeight = FontWeights.Bold
            };

            Border bubble = new()
            {
                Background = new SolidColorBrush( Color.FromRgb(0, 194, 255)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12)
            };

            StackPanel content = new();

            content.Children.Add(new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 16
                });

            content.Children.Add(new TextBlock
                {
                    Text = "✓✓ Sent",
                    FontSize = 10,
                    Foreground = Brushes.White,
                    HorizontalAlignment =  HorizontalAlignment.Right
                });

            bubble.Child = content;

            container.Children.Add(header);
            container.Children.Add(bubble);

            ChatPanel.Children.Add(container);
        }

        // Displays bot message bubble.
        private void AppendBotMessage(string message)
        {
            StackPanel container = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5)
            };

            TextBlock header = new()
            {
                Text =  $"🤖 Cybersecurity Assistant • {DateTime.Now:HH:mm}",
                Foreground =  Brushes.LightGray,
                FontWeight = FontWeights.Bold
            };

            Border bubble = new()
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 38, 58)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12)
            };

            bubble.Child =  new TextBlock
                {
                    Text = message,
                    Foreground = Brushes.White,
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap
                };

            container.Children.Add(header);
            container.Children.Add(bubble);

            ChatPanel.Children.Add(container);
        }

    }
}