using CybersecurityAwarenessChatbot.Classes;
using System.Windows;

namespace CybersecurityAwarenessChatbot
{
    public partial class LandingPage : Window
    {
        private readonly MemoryStore memoryStore;
        public LandingPage()
        {
            InitializeComponent();
            memoryStore = new MemoryStore();

            AsciiArtText.Text =
                UIHelper.ShowLogo();

            Loaded += LandingPage_Loaded;
        }

        private async void LandingPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            await Task.Run(() =>
                VoicePlayer.PlayGreeting());
        }

        private void ContinueButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            memoryStore.UserName =
                NameTextBox.Text.Trim();

            MessageBox.Show(
                $"Welcome {memoryStore.UserName}");
        }

        private void ChatButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ChatWindow window =
                new ChatWindow();

            window.Show();

            Close();
        }

        private void QuizButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            QuizWindow window = new QuizWindow(memoryStore.UserName);

            window.Show();

            Close();
        }

        private void TaskButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            TaskWindow window =
                new TaskWindow();

            window.Show();

            Close();
        }

        private void ExitButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}