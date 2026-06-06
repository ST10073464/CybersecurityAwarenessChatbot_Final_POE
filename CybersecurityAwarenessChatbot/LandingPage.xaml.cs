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
        public LandingPage()
        {
            InitializeComponent();
            memoryStore = new MemoryStore();

            UserNameText.Text = $"👤 {memoryStore.UserName}";

            AsciiArtText.Text = UIHelper.ShowLogo();

            Loaded += LandingPage_Loaded;
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

        // Displays bot messages in the landing page chat area.
        private void AppendBotMessage(string message)
        {
            Border bubble = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 38, 58)),
                CornerRadius = new CornerRadius(15),
                Padding = new Thickness(12),
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left,
                MaxWidth = 700
            };

            TextBlock text = new TextBlock
            {
                Text = $"🤖 SecureWin\n\n{message}",
                Foreground = Brushes.White,
                FontSize = 16,
                TextWrapping =
                TextWrapping.Wrap
            };

            bubble.Child = text;

            ChatPanel.Children.Add(bubble);
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
        
       // Handles user input
        private void SendMessage()
        {
            string input =UserInputTextBox.Text.Trim();

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
                memoryStore.UserName = formattedName;

                memoryStore.AddConversation($"User: {formattedName}");

                awaitingUserName = false;

                AppendBotMessage(
                    $"👋 Welcome {memoryStore.UserName}!\n\n" +
                    "I am SecureWin, your Cybersecurity Awareness Assistant.\n\n" +
                    "Please select one of the available options:\n\n" +
                    "Click one of the buttons below to continue.");

                UserInputTextBox.Clear();

                ChatScrollViewer.ScrollToBottom();

                return;
            }

            UserInputTextBox.Clear();

            ChatScrollViewer.ScrollToBottom();
        }
        // Send button click
        private void SendButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SendMessage();
        }
        // Send message when Enter pressed
        private void UserInputTextBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow window = new ChatWindow();


            window.Show();

            Close();
        }

        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            QuizWindow window = new QuizWindow(memoryStore.UserName);

            window.Show();

            Close();
        }

        private void TaskButton_Click(object sender, RoutedEventArgs e)
        {
            TaskWindow window = new TaskWindow();

            window.Show();

            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}