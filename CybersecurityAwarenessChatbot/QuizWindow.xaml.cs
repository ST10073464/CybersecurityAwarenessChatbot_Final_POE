/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using CybersecurityAwarenessChatbot.Models;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace CybersecurityAwarenessChatbot
{
    public partial class QuizWindow : Window
    {
        // Quiz service
        private readonly QuizService quizService;

        private readonly ChatBot _chatBot = new();

        // Stores wrong answers
        private readonly List<int> wrongQuestionIndexes;

        private List<QuizQuestion> originalQuestions;

        // Constructor
        public QuizWindow()
        {
            InitializeComponent();

            ActivityLogService.Add($"Quiz started by {MemoryStore.UserName} at {DateTime.Now:HH:mm:ss}");

            LoadActivityLog();

            quizService = new QuizService();

            wrongQuestionIndexes = new List<int>();

            originalQuestions = quizService.Questions
                .Select(q => new QuizQuestion
                {
                    Question = q.Question,
                    Options = new List<string>(q.Options),
                    CorrectAnswer = q.CorrectAnswer,
                    Explanation = q.Explanation
                })
                .ToList();

            // Display username
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            LoadQuestion();
        }

        // LOAD QUESTION
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
            QuestionText.Text = question.Question;

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
            ProgressText.Text = $"Question " +
                                $"{quizService.CurrentQuestionIndex + 1} " +
                                $"of {quizService.Questions.Count}";
        }

        // NEXT QUESTION
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            ActivityLogService.Add($"Question {quizService.CurrentQuestionIndex + 1}: Correct");
            ActivityLogService.Add($"Question {quizService.CurrentQuestionIndex + 1}: Incorrect");

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

            QuizQuestion question = quizService.Questions[quizService.CurrentQuestionIndex];

            // Correct answer selected
            if (selectedIndex == question.CorrectAnswer)
            {
                quizService.Score++;

                MessageBox.Show($"✅ Correct!\n\n" +
                           $"Answer: {question.Options[question.CorrectAnswer]}\n\n" +
                           $"{question.Explanation}",
                           "Correct Answer",
                           MessageBoxButton.OK,
                           MessageBoxImage.Information);
            }
            else
            {
                wrongQuestionIndexes.Add(quizService.CurrentQuestionIndex);

                MessageBox.Show($"❌ Incorrect!\n\n" +
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

        // SHOW RESULTS
        private void ShowResults()
        {
            QuestionText.Visibility = Visibility.Collapsed;
            OptionsPanel.Visibility = Visibility.Collapsed;
            NextButton.Visibility = Visibility.Collapsed;

            int correctAnswers = quizService.Score;
            int wrongAnswers = quizService.Questions.Count - quizService.Score;

            string badge = GetBadge();

            string results =
                $"🏁 QUIZ COMPLETE\n\n" +
                $"👤 User: {MemoryStore.UserName}\n\n" +
                $"✅ Correct Answers: {correctAnswers}\n" +
                $"❌ Wrong Answers: {wrongAnswers}\n\n" +
                $"📊 Score: {quizService.Score}/{quizService.Questions.Count}\n\n" +
                $"🏆 Badge: {badge}\n";

            ResultsText.Text = results;
            ResultsText.Visibility = Visibility.Visible;

            ActivityLogService.Add(
                $"Quiz completed. Score: {correctAnswers}/{quizService.Questions.Count}");

            if (wrongQuestionIndexes.Count > 0)
            {
                ShowCorrectAnswers();

                RetryButton.Visibility = Visibility.Visible;
                ViewAnswersButton.Visibility = Visibility.Collapsed;
            }
           /* else
            {
                ShowCorrectAnswers();

                TryAgainButton.Visibility = Visibility.Visible;

                ActivityLogService.Add($"Perfect score achieved at {DateTime.Now:HH:mm:ss}");
            }
           */
            LoadActivityLog();
        }
        private void ShowCorrectAnswers()
        {
            string results =
                $"📖 CORRECT ANSWERS\n\n";

            foreach (QuizQuestion q in originalQuestions)
            {
                results +=
                    $"✔ {q.Question}\n" +
                    $"Answer: {q.Options[q.CorrectAnswer]}\n\n";
            }

            ResultsText.Text = results;

            ResultsText.Visibility =
                Visibility.Visible;
        }

        private void ViewAnswersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowCorrectAnswers();
        }

        private void TryAgainButton_Click(object sender, RoutedEventArgs e)
        {
            quizService.Questions = originalQuestions
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            quizService.CurrentQuestionIndex = 0;

            quizService.Score = 0;

            wrongQuestionIndexes.Clear();

            ResultsText.Visibility =  Visibility.Collapsed;

            TryAgainButton.Visibility = Visibility.Collapsed;

            QuestionText.Visibility = Visibility.Visible;

            OptionsPanel.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Visible;

            ActivityLogService.Add($"New quiz attempt started at {DateTime.Now:HH:mm:ss}");

            LoadActivityLog();

            LoadQuestion();
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
                MessageBox.Show("You answered all questions correctly!");

                return;
            }

            List<QuizQuestion> retryQuestions = new List<QuizQuestion>();

            foreach (int index in wrongQuestionIndexes)
            {
                retryQuestions.Add( quizService.Questions[index]);
            }

            quizService.Questions = retryQuestions;

            quizService.CurrentQuestionIndex = 0;

            quizService.Score = 0;

            wrongQuestionIndexes.Clear();

            ResultsText.Visibility = Visibility.Collapsed;

            RetryButton.Visibility = Visibility.Collapsed;           

            QuestionText.Visibility = Visibility.Visible;

            OptionsPanel.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Visible;

            ActivityLogService.Add($"Quiz retry started at {DateTime.Now:HH:mm:ss}");

            LoadActivityLog();

            LoadQuestion();
        }

        private void LoadActivityLog()
        {
            ActivityLogText.Text = ActivityLogService.GetSummary();
        }

        // Back to main chat window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ChatWindow chatWindow = new ChatWindow();

            chatWindow.Show();

            Close();
        }
    }
}