/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace CybersecurityAwarenessChatbot
{
    public partial class QuizWindow : Window
    {
        // Quiz service
        private readonly QuizService quizService;

        // Stores wrong answers
        private readonly List<int> wrongQuestionIndexes;

        private int totalCorrectAnswers = 0;

        private int totalWrongAnswers = 0;

        private bool retryMode = false;

        private int totalQuestions;

        // Constructor
        public QuizWindow()
        {
            InitializeComponent();

            quizService = new QuizService();

            wrongQuestionIndexes = new List<int>();

            totalQuestions = quizService.Questions.Count;

            PreviewKeyDown += QuizWindow_PreviewKeyDown;

            // Display username
            UserNameText.Text = $"👤 {MemoryStore.UserName}";

            ResultsScrollViewer.PreviewMouseWheel += ResultsScrollViewer_PreviewMouseWheel;

            ActivityLogService.Add("QUIZ", $"{MemoryStore.UserName} started quiz");

            QuizActivityLogText.Text = ActivityLogService.GetLogs("QUIZ");

            Closing += QuizWindow_Closing;

            LoadActivityLog();

            LoadQuestion();
        }

        // Keyboard navigation for Next button
        private void QuizWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (NextButton.IsVisible && NextButton.IsEnabled && NextButton.IsFocused)
                {
                    NextButton_Click(NextButton, new RoutedEventArgs());

                    e.Handled = true;
                }
            }
        }

        private void ResultsScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ResultsScrollViewer.ScrollToVerticalOffset(ResultsScrollViewer.VerticalOffset - e.Delta);

            e.Handled = true;
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
                RadioButton option = new RadioButton
                    {
                        Content = question.Options[i],
                        Tag = i,
                        FontSize = 18,
                        Margin = new Thickness(5),
                        Foreground = Brushes.Black,
                        GroupName = "QuizOptions"
                    };

                OptionsPanel.Children.Add(option);
                option.KeyDown += Option_KeyDown;
            }

            // Progress display
            ProgressText.Text = $"Question " +
                                $"{quizService.CurrentQuestionIndex + 1} " +
                                $"of {quizService.Questions.Count}";
            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (OptionsPanel.Children.Count > 0)
                {
                    ((RadioButton)OptionsPanel.Children[0]).Focus();
                }
            }));
        }

        // Keyboard navigation for options
        private void Option_KeyDown(object sender, KeyEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                rb.IsChecked = true;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    NextButton.Focus();
                }));

                e.Handled = true;
            }
        }

        // NEXT QUESTION
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

            QuizQuestion question = quizService.Questions[quizService.CurrentQuestionIndex];

            // Correct answer selected
            if (selectedIndex == question.CorrectAnswer)
            {
                quizService.Score++;

                totalCorrectAnswers++;

                ActivityLogService.Add("QUIZ", $"Question {quizService.CurrentQuestionIndex + 1}: Correct");

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

                totalWrongAnswers++;

                ActivityLogService.Add("QUIZ", $"Question {quizService.CurrentQuestionIndex + 1}: Incorrect");

                MessageBox.Show($"❌ Incorrect!\n\n" +
                                $"Correct Answer:\n{question.Options[question.CorrectAnswer]}\n\n" +
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

            ProgressText.Visibility = Visibility.Collapsed;

            ResultsText.Visibility = Visibility.Visible;

            ResultsScrollViewer.Visibility = Visibility.Visible;

            ResultsText.Visibility = Visibility.Visible;

            // FIRST ATTEMPT COMPLETE
            if (!retryMode)
            {
                int correct = totalCorrectAnswers;

                int totalQuestions = quizService.Questions.Count;

                int wrong = totalWrongAnswers;
     
                string badge = GetBadge(correct, quizService.Questions.Count);

                string motivation = GetMotivationMessage(correct, quizService.Questions.Count);

                ResultsText.Text =
                    $"🏁 QUIZ COMPLETE\n\n" +
                    $"👤 User: {MemoryStore.UserName}\n\n" +
                    $"✅ Correct Answers: {correct}\n" +
                    $"❌ Wrong Answers: {wrong}\n\n" +
                    $"📊 Score: {correct}/{quizService.Questions.Count}\n\n" +
                    $"🏆 Badge: {badge}\n\n" +
                    $"{motivation}\n\n";

                // Perfect first attempt
                if (wrong == 0)
                {
                    ResultsText.Text += $"🌟 You answered every question correctly on your first attempt!\n\n" +
                                        $"Use 'View Answers' to review all correct answers or 'Try Full Quiz Again' for another challenge.";

                    ViewAnswersButton.Visibility = Visibility.Visible;
                    TryAgainButton.Visibility = Visibility.Visible;

                    ActivityLogService.Add("QUIZ", $"{MemoryStore.UserName} achieved a PERFECT SCORE ({correct}/{quizService.Questions.Count})");
                }
                else
                {
                    ResultsText.Text += $"🔄 You can now retry only the questions you answered incorrectly.\n\n" +
                                        $"Keep going — you're very close to a perfect score!";

                    RetryButton.Visibility = Visibility.Visible;
                    ViewAnswersButton.Visibility = Visibility.Visible;

                    ActivityLogService.Add("QUIZ", $"Quiz completed: {totalCorrectAnswers}/{totalQuestions}");
                }

                return;
            }

            // RETRY COMPLETE
            if (retryMode)
            {
                int finalCorrect = totalCorrectAnswers;

                int finalWrong = totalQuestions - finalCorrect;

                string badge = GetBadge(finalCorrect, totalQuestions);

                string motivation = GetMotivationMessage(finalCorrect, totalQuestions);

                ResultsText.Text =
                    $"🏁 QUIZ COMPLETE\n\n" +
                    $"👤 User: {MemoryStore.UserName}\n\n" +
                    $"✅ Correct Answers: {finalCorrect}\n" +
                    $"❌ Wrong Answers: {finalWrong}\n\n" +
                    $"📊 Score: {finalCorrect}/{totalQuestions}\n\n" +
                    $"🏆 Badge: {badge}\n\n" +
                    $"{motivation}\n\n";

                ActivityLogService.Add("QUIZ", $"Quiz retry started at {DateTime.Now:HH:mm:ss}");

                // PERFECT AFTER RETRY
                if (finalCorrect == totalQuestions)
                {
                    ResultsText.Text += $"🎉 PERFECT SCORE ACHIEVED!\n\n" +
                                        $"Excellent work! You corrected every mistake and achieved a perfect score.";

                    RetryButton.Visibility = Visibility.Collapsed;

                    ViewAnswersButton.Visibility = Visibility.Visible;

                    TryAgainButton.Visibility = Visibility.Visible;

                    ActivityLogService.Add("QUIZ", $"{MemoryStore.UserName} achieved PERFECT SCORE after retry");
                }
                else
                {
                    ResultsText.Text += $"💪 Keep going!\n\n" +
                                        $"Review the answers and try again.";

                    RetryButton.Visibility = Visibility.Visible;

                    ViewAnswersButton.Visibility = Visibility.Visible;
                }

                return;
            }

        }

        // Show correct answers in results
        private void ShowCorrectAnswers()
        {
            string results = "📖 CORRECT ANSWERS\n\n";

            foreach (QuizQuestion q in quizService.Questions)
            {
                results += $"✔ {q.Question}\n" +
                           $"Answer: {q.Options[q.CorrectAnswer]}\n\n";
            }

            ResultsText.Text = results;

            ResultsText.Visibility = Visibility.Visible;
        }

        // View correct answers button click
        private void ViewAnswersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowCorrectAnswers();
        }

        // Quiz badge based on score percentage
        private string GetBadge(int score, int total)
        {
            double percentage = (double)score / total * 100;

            if (percentage == 100)
                return "🥇 PERFECT";

            if (percentage >= 70)
                return "🥈 GOOD";

            return "🥉 TRY AGAIN";
        }

        // Motivational message based on score percentage
        private string GetMotivationMessage(int score, int total)
        {
            double percentage = (double)score / total * 100;

            if (percentage == 100)
            {
                return "🎉 Perfect Score Achieved!\n\n" +
                       "Outstanding work! You answered every cybersecurity question correctly.\n" +
                       "You have demonstrated excellent awareness of online safety practices.\n" +
                       "Keep applying these skills to stay safe online!";
            }

            if (percentage >= 70)
            {
                return "👏 Great Job!\n\n" +
                       "You have a strong understanding of cybersecurity concepts.\n" +
                       "Review the questions you missed and you'll be at a perfect score in no time.";
            }

            if (score == total)
            {
                return "🎉 PERFECT SCORE ACHIEVED!\n\n" +
                       "Excellent work! You corrected every mistake and achieved a perfect score.\n\n" +
                       "You have demonstrated excellent cybersecurity awareness and online safety knowledge.";
            }

            return "💪 Keep Learning!\n\n" +
                   "Every attempt improves your cybersecurity knowledge.\n" +
                   "Review the incorrect answers and try again.\n" +
                   "You can do it!";
        }

        // Retry only wrong questions
        private void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            if (wrongQuestionIndexes.Count == 0)
                return;

            retryMode = true;

            List<QuizQuestion> retryQuestions = new();

            foreach (int index in wrongQuestionIndexes)
            {
                retryQuestions.Add(
                    quizService.Questions[index]);
            }

            quizService.Questions = retryQuestions;

            quizService.CurrentQuestionIndex = 0;

            // Score only for retry questions
            quizService.Score = 0;

            ResultsText.Visibility = Visibility.Collapsed;

            RetryButton.Visibility = Visibility.Collapsed;

            ViewAnswersButton.Visibility = Visibility.Collapsed;

            TryAgainButton.Visibility = Visibility.Collapsed;

            QuestionText.Visibility = Visibility.Visible;

            OptionsPanel.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Visible;

            ProgressText.Visibility = Visibility.Collapsed;

            ResultsScrollViewer.Visibility = Visibility.Collapsed;

            ResultsText.Visibility = Visibility.Collapsed;

            ActivityLogService.Add("QUIZ", $"Quiz Retry Started at {DateTime.Now:HH:mm:ss}");

            LoadQuestion();
        }

        private void TryAgainButton_Click(object sender, RoutedEventArgs e)
        {
            retryMode = false;

            totalCorrectAnswers = 0;

            totalWrongAnswers = 0;

            wrongQuestionIndexes.Clear();

            quizService.Questions = quizService.Questions
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            quizService.CurrentQuestionIndex = 0;

            quizService.Score = 0;

            ResultsText.Visibility = Visibility.Collapsed;

            RetryButton.Visibility = Visibility.Collapsed;

            ViewAnswersButton.Visibility = Visibility.Collapsed;

            TryAgainButton.Visibility = Visibility.Collapsed;

            QuestionText.Visibility = Visibility.Visible;

            OptionsPanel.Visibility = Visibility.Visible;

            NextButton.Visibility = Visibility.Visible;

            ProgressText.Visibility = Visibility.Collapsed;

            ResultsScrollViewer.Visibility = Visibility.Collapsed;

            ResultsText.Visibility = Visibility.Collapsed;

            ActivityLogService.Add("QUIZ", $"Full Quiz Restarted at {DateTime.Now:HH:mm:ss}");

            LoadQuestion();
        }

        private void LoadActivityLog()
        {
            QuizActivityLogText.Text = ActivityLogService.GetAllLogs();
        }

        private void QuizWindow_Closing(object sender, CancelEventArgs e)
        {
            ActivityLogService.Add(
                "QUIZ",
                $"{MemoryStore.UserName} closed Quiz Window");
        }

        // Back to main chat window
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow chatWindow = new MainWindow();

            chatWindow.Show();

            Close();
        }
    }
}