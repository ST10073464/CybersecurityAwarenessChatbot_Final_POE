/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

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
           
            AsciiArtText.Text = UIHelper.ShowLogo();

            Loaded += ChatWindow_Loaded;

        }

        // Opens task management windows based on mode (ADD, VIEW, DELETE, COMPLETE).
        private void OpenTaskWindow(string mode)
        {
            TaskWindow taskWindow = new TaskWindow(mode);

            taskWindow.Show();

            Close();
        }

        // On window load, display welcome message and activity log summary.
        private void ChatWindow_Loaded(object sender, RoutedEventArgs e)
        {

            AppendBotMessage(_chatBot.ProcessInput(""));
            
            // Focus textbox automatically
            UserInputTextBox.Focus();

        }
        // Send user message when Send button is clicked.
        private void SendMessage()
        {
            string input = UserInputTextBox.Text.ToLower().Trim();

            ActivityLogService.Add($"{MemoryStore.UserName} sent message: {input}");

            if (string.IsNullOrWhiteSpace(input))
                return;

            // Show user message
            AppendUserMessage(input);

            // Process through chatbot
            string response = _chatBot.ProcessInput(input);

            UserInputTextBox.Clear();

            switch (response)
            {
                case "__OPEN_TASK_ADD__":

                    ActivityLogService.Add($"Opened Add Task by {MemoryStore.UserName} at {DateTime.Now:HH:mm:ss}");

                    new TaskWindow("ADD").Show();

                    Close();

                    return;

                case "__OPEN_TASK_VIEW__":

                    ActivityLogService.Add($"Viewed Tasks by {MemoryStore.UserName} at {DateTime.Now:HH:mm:ss}");

                    new TaskWindow("VIEW").Show();

                    Close();

                    return;

                case "__OPEN_TASK_DELETE__":

                    ActivityLogService.Add($"Delete Task Requested by {MemoryStore.UserName} at {DateTime.Now:HH:mm:ss}");

                    new TaskWindow("DELETE").Show();

                    Close();

                    return;

                case "__OPEN_TASK_COMPLETE__":

                    ActivityLogService.Add($"Complete Task Requested by {MemoryStore.UserName} at {DateTime.Now:HH:mm:ss}");

                    new TaskWindow("COMPLETE").Show();

                    Close();

                    return;

                case "__OPEN_QUIZ__":

                    ActivityLogService.Add($"Quiz Started by {MemoryStore.UserName} at {DateTime.Now:HH:mm:ss}");

                    new QuizWindow().Show();

                    Close();

                    return;

                case $"__SHOW_ACTIVITY_LOG__" :

                    AppendBotMessage(ActivityLogService.GetSummary());

                    return;
            }

            // Normal chatbot response
            if (!string.IsNullOrWhiteSpace(response))
            {
                AppendBotMessage(response);
            }
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

        private void ViewLogsButton_Click(object sender, RoutedEventArgs e)
        {
            AppendBotMessage(ActivityLogService.GetSummary());
        }

        // End current session and allow another user to log in.
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                                      "Are you sure you want to leave this session?",
                                      "Leave Session",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MemoryStore.UserName = "";

                LandingPage landing = new LandingPage();

                landing.Show();

                Close();
            }

        }

    }
}