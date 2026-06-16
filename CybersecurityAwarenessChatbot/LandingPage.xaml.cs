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
        private readonly MemoryStore memoryStore = new();

        // Chatbot instance for processing user input
        private readonly ChatBot _chatBot = new();

        // Tracks whether username has been entered
        private bool awaitingUserName = true;

        // Flag to ensure greeting is only played once
        private bool _isGreetingPlayed = false;

        // Constructor 
        public LandingPage()
        {
            InitializeComponent();

            // Initialize chatbot
            _chatBot = new ChatBot();

            // Disable textbox during startup greeting
            UserInputTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            memoryStore = new MemoryStore();

            // Load ASCII art
            AsciiArtText.Text = UIHelper.ShowLogo();
        }

        
        // Landing page startup
        private async void LandingPage_Loaded(object sender, RoutedEventArgs e)
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

            // Focus textbox automatically
            UserInputTextBox.Focus();

        }

        // Handles send button click.
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // Handles placeholder text behavior.
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PlaceholderText.Visibility =
                string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        // Allows Enter key to send messages.
        private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
            }
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
                string formattedName =
                    string.Join(" ",
                        input.ToLower()
                             .Split(' ',
                             StringSplitOptions.RemoveEmptyEntries)
                             .Select(word =>
                                 char.ToUpper(word[0]) +
                                 word.Substring(1)));

                MemoryStore.UserName = formattedName;

                memoryStore.AddConversation($"User: {formattedName}");

                awaitingUserName = false;

                UserInputTextBox.Clear();

                ChatWindow chatWindow = new ChatWindow();

                chatWindow.Show();

                this.Close();

                return;
            }

            UserInputTextBox.Clear();

            ChatScrollViewer.ScrollToBottom();
        }

        // End current session and allow another user to log in.
        private void LeaveSessionButton_Click(object sender, RoutedEventArgs e)
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

        // Exit button click - shuts down the application
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}