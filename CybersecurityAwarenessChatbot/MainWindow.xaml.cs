/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Services;
using System.IO;
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
        private readonly ChatBot _chatBot;
        private bool _isGreetingPlayed = false;
        private bool awaitingUserName = true;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // Pull the saved session history from local file storage
            MemoryStore.LoadSession();

            // Default fallback display setup 
            UserNameText.Text = $"👤 {MemoryStore.UserName ?? "Guest"}";

            // Initialize chatbot engine
            _chatBot = new ChatBot();

            // Disable textbox during startup greeting sequencing
            UserInputTextBox.IsEnabled = false;
            SendButton.IsEnabled = false;

            // Load ASCII art logo
            AsciiArtText.Text = UIHelper.ShowLogo();

            // Run startup layout lifecycle sequencer
            Loaded += MainWindow_Loaded;
        }

        // On window load, display welcome message and activity log summary.
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Check if it already ran to prevent duplicated lifecycle pings
            if (_isGreetingPlayed) return;
            _isGreetingPlayed = true;

            // Play greeting voice asynchronously
            await Task.Run(() => VoicePlayer.PlayGreeting());

            // Play a quick system notification ping
            System.Media.SystemSounds.Asterisk.Play();

            // Show chatbot greeting after audio finishes
            AppendBotMessage(_chatBot.GetGreeting());

            // Enable user interaction input elements
            UserInputTextBox.IsEnabled = true;
            SendButton.IsEnabled = true;

            if (string.IsNullOrWhiteSpace(MemoryStore.UserName))
            {
                MemoryStore.UserName = "Guest";
            }

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Load recent layout elements into UI panel
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

        // Load recent activity log into the ActivityLogText TextBox (Strictly limited to a maximum of 10 items)
        private void LoadRecentActivity()
        {
            try
            {
                // Retrieve all records from the storage log layer
                var logString = ActivityLogService.GetAllLogs();
                if (string.IsNullOrWhiteSpace(logString))
                {
                    ActivityLogText.Text = "No activities recorded yet.";
                    return;
                }

                var rawLogs = logString
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                // Display only the latest 10 items directly onto the UI layout sidebar view
                var truncatedLogs = rawLogs.TakeLast(10);
                ActivityLogText.Text = string.Join(Environment.NewLine, truncatedLogs);
            }
            catch
            {
                ActivityLogText.Text = "Error loading activity logs.";
            }
        }

        // Send user message and route business rules parsing logic responses
        private void SendMessage()
        {
            string input = UserInputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            // Play input execution notification sound
            VoicePlayer.PlaySound();

            // Format message casing safely for cleaner layout rendering
            input = char.ToUpper(input[0]) + input.Substring(1);

            // Display user message bubble right-aligned
            AppendUserMessage(input);

            // Let chatbot parse and capture logs internally
            string response = _chatBot.ProcessInput(input);

            // Synchronize State settings from memory parameters safely
            if (awaitingUserName && MemoryStore.UserName != "Guest")
            {
                awaitingUserName = false;
            }

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            // Check if string contains systemic cleanup clear signals
            if (response.StartsWith("__CLEAR_CHAT__"))
            {
                ChatPanel.Children.Clear();
                response = response.Replace("__CLEAR_CHAT__", "").Trim();
            }

            // Route execution strings targeting application view modifications
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
                    Hide();
                    return;

                case "__SHOW_ACTIVITY_LOG__":
                    AppendBotMessage(ActivityLogService.GetAllLogs());
                    UserInputTextBox.Clear();
                    LoadRecentActivity();
                    return;

                case "__OPEN_TASK_REMINDERS__":
                    new TaskWindow("REMINDERS").Show();
                    Close();
                    return;

                case "__LEAVE_SESSION__":
                    string currentUser = UserNameText.Text.Replace("👤", "").Trim();

                    MessageBox.Show(
                        $"{currentUser} ended the chat.",
                        "Session Ended",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Reset session flags on UI level
                    awaitingUserName = true;
                    MemoryStore.UserName = "Guest";
                    UserNameText.Text = "👤 Guest";

                    // Clear existing chat flow views
                    ChatPanel.Children.Clear();

                    // Reload clean baseline environment greets
                    AppendBotMessage(_chatBot.GetGreeting());
                    LoadRecentActivity();

                    UserInputTextBox.Clear();
                    UserInputTextBox.Focus();
                    return;

                case "__CLOSE_SECUREWIN__":
                    MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to close SecureWin?",
                        "Close SecureWin",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Save everything right before shutdown execution runs
                        MemoryStore.SaveSession();
                        MemoryStore.UserName = "";
                        Application.Current.Shutdown();
                    }
                    UserInputTextBox.Clear();
                    return;
            }

            // Standard message fallback printing flow
            if (!string.IsNullOrEmpty(response))
            {
                AppendBotMessage(response);
            }

            // Clear input box and scroll to baseline frame container bounds
            UserInputTextBox.Clear();

            Dispatcher.InvokeAsync(() =>
            {
                ChatScrollViewer.ScrollToEnd();
            });

            // Re-sync visual side panels to maintain real-time tracking accuracy
            LoadRecentActivity();
            UserInputTextBox.Focus();
        }

        // Placeholder text visibility based on user input lengths
        private void UserInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaceholderText != null)
            {
                PlaceholderText.Visibility = string.IsNullOrWhiteSpace(UserInputTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Hidden;
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

            Dispatcher.InvokeAsync(() => { ChatScrollViewer.ScrollToEnd(); });
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

            Dispatcher.InvokeAsync(() => { ChatScrollViewer.ScrollToEnd(); });
        }

        private void ShowSavedJsonTask()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tasks.json");

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

            AppendBotMessage("📁 SAVED TASKS (JSON)\n\n" + json);
        }

        private void ViewJsonTasksButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSavedJsonTask();
            LoadRecentActivity();
        }

        private void ViewLogsButton_Click(object sender, RoutedEventArgs e)
        {
            // Log the button click action context
            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} requested master system logs view");

            // Pull down everything combined (MAIN, TASK, and QUIZ)
            ActivityLogText.Text = ActivityLogService.GetAllLogs();
        }
    }
}