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
    public partial class LandingPage : Window
    {
        private readonly MemoryStore memoryStore;

        // Tracks whether username has been entered
        private bool awaitingUserName = true;

        // Prevent greeting from playing twice
        private bool _isGreetingPlayed = false;

        // Constructor 
        public LandingPage()
        {
            InitializeComponent();

            memoryStore = new MemoryStore();

            AsciiArtText.Text = UIHelper.ShowLogo();

            Loaded += LandingPage_Loaded;

            if (!string.IsNullOrWhiteSpace(MemoryStore.UserName))
            {
                ShowLoggedInView();
            }
        }

        private void ShowLoggedInView()
        {
            UserInputTextBox.Text = $"👤 {MemoryStore.UserName}";

            WelcomeText.Text = $"👋 Welcome {MemoryStore.UserName}\n\n" +
                               "Please select one of the options below.";

            WelcomeText.Visibility = Visibility.Visible;

            ChatArea.Visibility = Visibility.Collapsed;

            InputArea.Visibility = Visibility.Collapsed;

            ModeButtonsPanel.Visibility = Visibility.Visible;
        }

        // Landing page startup
        private async void LandingPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Prevent duplicate execution
            if (_isGreetingPlayed)
                return;

            _isGreetingPlayed = true;

            // Play greeting audio once
            await Task.Run(() => VoicePlayer.PlayGreeting());

            // Show chatbot greeting
            AppendBotMessage(
                "Welcome to SecureWin Cybersecurity Awareness Assistant.\n\n" +
                "Please enter your name to continue.");

            UserInputTextBox.Focus();
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

        // Displays bot message bubble with header and timestamp, styled differently from user messages.
        private void AppendBotMessage(string message)
        {
            StackPanel container = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5)
            };

            TextBlock header = new TextBlock
            {
                Text = $"🤖 SecureWin • {DateTime.Now:HH:mm}",
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
        }

        // Placeholder text visibility based on user input
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrWhiteSpace( UserInputTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        // Handles user input
        private void SendMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            // Show user message
            AppendUserMessage(input);

            // Ask for username

            if (awaitingUserName)
            {
                // Format name
                string formattedName =
                    string.Join(" ",
                        input.ToLower()
                             .Split(' ',
                             StringSplitOptions.RemoveEmptyEntries)
                             .Select(word =>
                                 char.ToUpper(word[0]) +
                                 word.Substring(1)));

                // Save globally
                MemoryStore.UserName = formattedName;

                // Hide chat area
                ChatArea.Visibility = Visibility.Collapsed;

                // Hide input area
                InputArea.Visibility = Visibility.Collapsed;

                // Show welcome message
                WelcomeText.Text =
                    $"👋 Welcome {MemoryStore.UserName}\n\n" +
                    "Please select one of the options below.";

                WelcomeText.Visibility = Visibility.Visible;

                // Show mode buttons
                ModeButtonsPanel.Visibility = Visibility.Visible;

                // Show mode buttons
                ModeButtonsPanel.Visibility = Visibility.Visible;

                memoryStore.AddConversation($"User: {formattedName}");

                awaitingUserName = false;

                UserInputTextBox.Clear();

                ChatScrollViewer.ScrollToBottom();

                return;
            }

            UserInputTextBox.Clear();

            ChatScrollViewer.ScrollToBottom();
        }

        // Send button click
        private void SendButton_Click( object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Send message when Enter pressed
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        // Chat button click - opens chat window and closes landing page
        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow window = new ChatWindow();

            window.Show();

            Close();
        }

        // Quiz button click - opens quiz window and closes landing page
        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            QuizWindow window = new QuizWindow();

            window.Show();

            Close();
        }

        // Task button click - opens task window and closes landing page
        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            TaskWindow window = new TaskWindow();

            window.Show();

            Close();
        }

        // End current session and allow another user to log in.
        private void LeaveSessionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                                    "Are you sure you want to leave this session?",
                                    "Leave Session",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ResetLandingPage();
            }
        }

        // Reset the landing page for a new user.
        private void ResetLandingPage()
        {
            // Clear stored user
            MemoryStore.UserName = "";

            // Allow username entry again
            awaitingUserName = true;

            // Show chat area
            ChatArea.Visibility = Visibility.Visible;

            // Show input area
            InputArea.Visibility = Visibility.Visible;

            // Hide welcome text
            WelcomeText.Visibility = Visibility.Collapsed;

            // Hide buttons until new user enters name
            ModeButtonsPanel.Visibility = Visibility.Collapsed;

            // Clear conversation display
            ChatPanel.Children.Clear();

            // Clear input
            UserInputTextBox.Clear();

            // Reset username label
            UserInputTextBox.Text = $"{MemoryStore.UserName}";

            // Show initial greeting
            AppendBotMessage(
                "Welcome to SecureWin Cybersecurity Awareness Assistant.\n\n" +
                "Please enter your name to continue.");

            UserInputTextBox.Focus();
        }



        // Exit button click - shuts down the application
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}