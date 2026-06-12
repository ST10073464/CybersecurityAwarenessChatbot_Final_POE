using CybersecurityAwarenessChatbot.Classes;
using System.Windows;
using System.Windows.Controls;

namespace CybersecurityAwarenessChatbot
{
    public partial class QuizWindow : Window
    {
        // Quiz service
        private readonly QuizService quizService;

        private readonly MemoryStore memoryStore = new();

        // Stores wrong answers for retry
        private readonly List<int> wrongQuestionIndexes;

        // Current retry mode
        private bool retryMode = false;

        public QuizWindow()
        {
            InitializeComponent();

            quizService = new QuizService();

            wrongQuestionIndexes = new List<int>();

            UserNameText.Text = $"👤 {MemoryStore.UserName}";

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

            QuizQuestion question = quizService.Questions[quizService.CurrentQuestionIndex];

            QuestionText.Text = question.Question;

            AnswerListBox.Items.Clear();

            foreach (string option
                in question.Options)
            {
                AnswerListBox.Items.Add(option);
            }

            ProgressText.Text =
                        $"Question " +
                        $"{quizService.CurrentQuestionIndex + 1}" +
                        $" of {quizService.Questions.Count}";
        }

        // =====================================================
        // NEXT BUTTON
        // =====================================================
        private void NextButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (AnswerListBox.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Please select an answer.");

                return;
            }

            QuizQuestion question =
                quizService.Questions[quizService.CurrentQuestionIndex];

            if (AnswerListBox.SelectedIndex == question.CorrectAnswer)
            {
                quizService.Score++;
            }
            else
            {
                wrongQuestionIndexes.Add(quizService.CurrentQuestionIndex);
            }

            quizService.CurrentQuestionIndex++;

            LoadQuestion();
        }

        // =====================================================
        // SHOW RESULTS
        // =====================================================
        private void ShowResults()
        {
            QuestionText.Visibility = Visibility.Collapsed;

            AnswerListBox.Visibility = Visibility.Collapsed;

            NextButton.Visibility = Visibility.Collapsed;

            string badge = GetBadge();

            string results =
                            $"🏁 Quiz Complete\n\n" +

                            $"👤 User: {MemoryStore.UserName}\n\n" +

                            $"📊 Score: " +
                            $"{quizService.Score}/" +
                            $"{quizService.Questions.Count}\n\n" +

                            $"🏆 Badge: {badge}\n\n" +

                            $"📖 Correct Answers:\n\n";

            foreach (QuizQuestion q
                in quizService.Questions)
            {
                results +=
                    $"✔ {q.Question}\n" +
                    $"Answer: " +
                    $"{q.Options[q.CorrectAnswer]}\n\n";
            }

            ResultsText.Text = results;

            ResultsText.Visibility = Visibility.Visible;

            RetryButton.Visibility = Visibility.Visible;

            BackButton.Visibility = Visibility.Visible;
        }

        // =====================================================
        // QUIZ BADGES
        // =====================================================
        private string GetBadge()
        {
            double percent =
                (double)quizService.Score /
                quizService.Questions.Count * 100;

            if (percent == 100)
                return "🥇 PERFECT";

            if (percent >= 70)
                return "🥈 GOOD";

            return "🥉 TRY AGAIN";
        }

        // =====================================================
        // RETRY WRONG QUESTIONS
        // =====================================================
        private void RetryButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (wrongQuestionIndexes.Count == 0)
            {
                MessageBox.Show(
                    "You answered all questions correctly!");

                return;
            }

            List<QuizQuestion> retryQuestions = new List<QuizQuestion>();

            foreach (int index in wrongQuestionIndexes)
            {
                retryQuestions.Add(quizService.Questions[index]);
            }

            quizService.Questions = retryQuestions;

            quizService.CurrentQuestionIndex = 0;

            quizService.Score = 0;

            wrongQuestionIndexes.Clear();

            retryMode = true;

            ResultsText.Visibility = Visibility.Collapsed;

            RetryButton.Visibility = Visibility.Collapsed;

            BackButton.Visibility = Visibility.Collapsed;

            QuestionText.Visibility = Visibility.Visible;

            AnswerListBox.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Visible;

            LoadQuestion();
        }

        // =====================================================
        // BACK TO MENU
        // =====================================================
        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            LandingPage landing = new LandingPage();

            landing.Show();

            Close();
        }
    }

}
