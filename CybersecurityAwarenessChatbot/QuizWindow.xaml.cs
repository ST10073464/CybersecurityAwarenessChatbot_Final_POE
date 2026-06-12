/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CybersecurityAwarenessChatbot
{
    public partial class QuizWindow : Window
    {
        // Quiz service
        private readonly QuizService quizService;

        // Stores wrong answers
        private readonly List<int> wrongQuestionIndexes;

        // Retry mode flag
        private bool retryMode = false;

        // Constructor
        public QuizWindow()
        {
            InitializeComponent();

            quizService = new QuizService();

            wrongQuestionIndexes = new List<int>();

            // Display username
            UserNameText.Text =
                $"👤 {MemoryStore.UserName}";

            LoadQuestion();
        }

        // =====================================================
        // LOAD QUESTION
        // =====================================================
        private void LoadQuestion()
        {
            // Quiz finished
            if (quizService.CurrentQuestionIndex >= quizService.Questions.Count)
            {
                ShowResults();
                return;
            }

            QuizQuestion question = quizService.Questions[quizService.CurrentQuestionIndex];

            // Display question
            QuestionText.Text =
                question.Question;

            // Clear previous radio buttons
            OptionsPanel.Children.Clear();

            // Create radio buttons
            for (int i = 0; i < question.Options.Count; i++)
            {
                RadioButton option =
                    new RadioButton
                    {
                        Content = question.Options[i],
                        Tag = i,
                        FontSize = 18,
                        Margin = new Thickness(5),
                        Foreground = Brushes.Black,
                        GroupName = "QuizOptions"
                    };

                OptionsPanel.Children.Add(option);
            }

            // Progress display
            ProgressText.Text =
                $"Question " +
                $"{quizService.CurrentQuestionIndex + 1} " +
                $"of {quizService.Questions.Count}";
        }

        // =====================================================
        // NEXT QUESTION
        // =====================================================
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = -1;

            // Find selected radio button
            foreach (RadioButton rb in OptionsPanel.Children)
            {
                if (rb.IsChecked == true)
                {
                    selectedIndex = (int)rb.Tag;

                    break;
                }
            }

            // No answer selected
            if (selectedIndex == -1)
            {
                MessageBox.Show("Please select an answer.");

                return;
            }

            QuizQuestion question =
                quizService.Questions[quizService.CurrentQuestionIndex];

            // Correct answer selected
            if (selectedIndex == question.CorrectAnswer)
            {
                quizService.Score++;

                MessageBox.Show(
                    $"✅ Correct!\n\n" +
                    $"Answer: {question.Options[question.CorrectAnswer]}\n\n" +
                    $"{question.Explanation}",
                    "Correct Answer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                wrongQuestionIndexes.Add(quizService.CurrentQuestionIndex);

                MessageBox.Show(
                    $"❌ Incorrect!\n\n" +
                    $"Correct Answer:\n" +
                    $"{question.Options[question.CorrectAnswer]}\n\n" +
                    $"{question.Explanation}",
                    "Incorrect Answer",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Move to next question
            quizService.CurrentQuestionIndex++;

            LoadQuestion();
        }

        // =====================================================
        // SHOW RESULTS
        // =====================================================
        private void ShowResults()
        {
            // Hide quiz controls
            PlaceholderText.Visibility = Visibility.Collapsed;

            QuestionText.Visibility = Visibility.Collapsed;

            OptionsPanel.Visibility =  Visibility.Collapsed;

            NextButton.Visibility = Visibility.Collapsed;

            string badge = GetBadge();

            string results =
                $"🏁 Quiz Complete\n\n" +

                $"👤 User: " +
                $"{MemoryStore.UserName}\n\n" +

                $"📊 Score: " +
                $"{quizService.Score}/" +
                $"{quizService.Questions.Count}\n\n" +

                $"🏆 Badge: " +
                $"{badge}\n\n" +

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

        // Quiz badge based on score percentage
        private string GetBadge()
        {
            double percentage = (double)quizService.Score / quizService.Questions.Count * 100;

            if (percentage == 100)
                return "🥇 PERFECT";

            if (percentage >= 70)
                return "🥈 GOOD";

            return "🥉 TRY AGAIN";
        }

        // Retry quiz with only wrong questions
        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (wrongQuestionIndexes.Count == 0)
            {
                MessageBox.Show( "You answered all questions correctly!");

                return;
            }

            List<QuizQuestion> retryQuestions = new List<QuizQuestion>();

            foreach (int index
                in wrongQuestionIndexes)
            {
                retryQuestions.Add( quizService.Questions[index]);
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

            OptionsPanel.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Visible;

            PlaceholderText.Visibility = Visibility.Visible;

            LoadQuestion();
        }

        // Back to landing page
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            LandingPage landing = new LandingPage();

            landing.Show();

            Close();
        }
    }
}