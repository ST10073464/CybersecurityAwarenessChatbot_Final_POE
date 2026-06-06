using CybersecurityAwarenessChatbot.Classes;
using System.Text;
using System.Windows;

namespace CybersecurityAwarenessChatbot
{
    public partial class QuizWindow : Window
    {
        private readonly QuizService quizService;
        private readonly ActivityLogService logService;
        private readonly MemoryStore memoryStore = new();
        private readonly string _userName;

        private List<int> userAnswers = new();

        public string UserName { get; set; }

        public QuizWindow(string userName)
        {
            InitializeComponent();

            UserNameText.Text = $"👤 {memoryStore.UserName}";

            quizService = new QuizService();
            logService = new ActivityLogService();

            LoadQuestion();
        }

        // =====================================================
        // LOAD QUESTION
        // =====================================================

        private void LoadQuestion()
        {
            if (quizService.CurrentQuestionIndex >=
                quizService.Questions.Count)
            {
                ShowResults();
                return;
            }

            QuizQuestion question =
                quizService.Questions[
                    quizService.CurrentQuestionIndex];

            QuestionText.Text =
                $"Question {quizService.CurrentQuestionIndex + 1}\n\n" +
                question.Question;

            Option1Button.Visibility = Visibility.Collapsed;
            Option2Button.Visibility = Visibility.Collapsed;
            Option3Button.Visibility = Visibility.Collapsed;
            Option4Button.Visibility = Visibility.Collapsed;

            if (question.Options.Count > 0)
            {
                Option1Button.Content = question.Options[0];
                Option1Button.Visibility = Visibility.Visible;
            }

            if (question.Options.Count > 1)
            {
                Option2Button.Content = question.Options[1];
                Option2Button.Visibility = Visibility.Visible;
            }

            if (question.Options.Count > 2)
            {
                Option3Button.Content = question.Options[2];
                Option3Button.Visibility = Visibility.Visible;
            }

            if (question.Options.Count > 3)
            {
                Option4Button.Content = question.Options[3];
                Option4Button.Visibility = Visibility.Visible;
            }
        }

        // =====================================================
        // ANSWER CLICK
        // =====================================================

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            int answer = 0;

            if (sender == Option1Button) answer = 0;
            if (sender == Option2Button) answer = 1;
            if (sender == Option3Button) answer = 2;
            if (sender == Option4Button) answer = 3;

            userAnswers.Add(answer);

            QuizQuestion current =
                quizService.Questions[
                    quizService.CurrentQuestionIndex];

            if (answer == current.CorrectAnswer)
            {
                quizService.Score++;
            }

            quizService.CurrentQuestionIndex++;

            LoadQuestion();
        }

        // =====================================================
        // SHOW RESULTS
        // =====================================================

        private void ShowResults()
        {
            Option1Button.Visibility = Visibility.Collapsed;
            Option2Button.Visibility = Visibility.Collapsed;
            Option3Button.Visibility = Visibility.Collapsed;
            Option4Button.Visibility = Visibility.Collapsed;


            string badge;

            if (quizService.Score ==
                quizService.Questions.Count)
            {
                badge = "🏆 PERFECT";
            }
            else if
            (
                quizService.Score >=
                quizService.Questions.Count / 2
            )
            {
                badge = "🥈 GOOD";
            }
            else
            {
                badge = "🔄 TRY AGAIN";
            }

            StringBuilder results =
                new StringBuilder();

            results.AppendLine(
                $"Well done, {UserName}!\n");

            results.AppendLine(
                $"Score: {quizService.Score}/{quizService.Questions.Count}");

            results.AppendLine(
                $"Badge: {badge}\n");

            results.AppendLine(
                "Correct Answers:\n");

            foreach (QuizQuestion question in quizService.Questions)
            {
                results.AppendLine(
                    $"• {question.Question}");

                results.AppendLine(
                    $"Answer: {question.Options[question.CorrectAnswer]}");

                results.AppendLine(
                    $"{question.Explanation}\n");
            }


            QuestionText.Text = "🎉 Quiz Complete";

            ResultText.Text = results.ToString();

            BackToQuizButton.Visibility = Visibility.Visible;
            ResultButtons.Visibility = Visibility.Visible;

            logService.AddLog(
            $"{UserName} completed quiz. Score: {quizService.Score}");
        }

        // Reset quiz state but stay in QuizWindow

            protected override void OnClosing(
        System.ComponentModel.CancelEventArgs e)
            {
                e.Cancel = true;

                QuizWindow quizWindow = new QuizWindow(_userName);

                quizWindow.Show();

                Hide();
            }
        private void BackToQuizButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset quiz state but stay in QuizWindow
            quizService.CurrentQuestionIndex = 0;
            quizService.Score = 0;

            userAnswers.Clear();

            ResultText.Text = "";

            Option1Button.Visibility = Visibility.Visible;
            Option2Button.Visibility = Visibility.Visible;
            Option3Button.Visibility = Visibility.Visible;
            Option4Button.Visibility = Visibility.Visible;

            QuestionText.Text = "";

            // Reload first question
            LoadQuestion();
        }

        // =====================================================
        // RETRY QUIZ
        // =====================================================

        private void RetryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            quizService.CurrentQuestionIndex = 0;
            quizService.Score = 0;

            userAnswers.Clear();

            ResultText.Text = "";

            LoadQuestion();
        }

        // =====================================================
        // BACK TO MENU
        // =====================================================

        private void MenuButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LandingPage menu = new LandingPage();

            menu.Show();

            Close();
        }
    }
}