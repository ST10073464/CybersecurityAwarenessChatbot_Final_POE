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

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Show username in header
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Initialize chatbot
            _chatBot = new ChatBot();
           
            AsciiArtText.Text = UIHelper.ShowLogo();

            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} started chatbot");

            Loaded += MainWindow_Loaded;

        }

        // On window load, display welcome message and activity log summary.
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            AppendBotMessage(_chatBot.ProcessInput(""));
            
            // Focus textbox automatically
            UserInputTextBox.Focus();

        }
        // Send user message when Send button is clicked.
        private void SendMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            // Log user activity
            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} sent message: '{input}'");

            // Display user message
            AppendUserMessage(input);

            // Clear textbox immediately
            UserInputTextBox.Clear();

            // Process chatbot response
            string response = _chatBot.ProcessInput(input);

            switch (response)
            {
                case "__OPEN_TASK_ADD__":

                    new TaskWindow("ADD").Show();
                    Close();
                    return;

                case "__OPEN_TASK_VIEW__":

                    new TaskWindow("VIEW").Show();
                    Close();
                    return;

                case "__OPEN_TASK_DELETE__":

                    new TaskWindow("DELETE").Show();
                    Close();
                    return;

                case "__OPEN_TASK_COMPLETE__":

                    new TaskWindow("COMPLETE").Show();
                    Close();
                    return;

                case "__OPEN_QUIZ__":

                    new QuizWindow().Show();
                    Close();
                    return;

                case "__SHOW_ACTIVITY_LOG__":

                    AppendBotMessage(ActivityLogService.GetAllLogs());

                    break;

                case "__LEAVE_SESSION__":

                    MessageBoxResult result =
                        MessageBox.Show(
                            "Are you sure you want to leave the session?",
                            "Leave Session",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        MemoryStore.UserName = "";

                        new LandingPage().Show();

                        Close();
                    }

                    return;

            }

            // Scroll to latest message
            Dispatcher.InvokeAsync(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            });

            // Return focus to textbox
            UserInputTextBox.Focus();
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