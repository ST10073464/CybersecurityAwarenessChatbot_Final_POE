/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    public class QuizService
    {
        public List<QuizQuestion> Questions { get; set; }

        public int CurrentQuestionIndex { get; set; }

        public int Score { get; set; }

        // Constructor initializes the quiz questions
        public QuizService()
        {
            Questions = new List<QuizQuestion>()
            {
                new QuizQuestion
                {
                    Question="What should you do with suspicious emails?",
                    Options=new List<string>
                    {
                        "Open attachments",
                        "Report phishing",
                        "Reply immediately",
                        "Ignore"
                    },
                    CorrectAnswer=1,
                    Explanation="Reporting phishing helps prevent scams."
                },

                new QuizQuestion
                {
                    Question="True or False: Password123 is secure.",
                    Options=new List<string>
                    {
                        "True",
                        "False"
                    },
                    CorrectAnswer=1,
                    Explanation="Weak passwords are easy to crack."
                },

                new QuizQuestion
                {
                    Question="What does 2FA mean?",
                    Options=new List<string>
                    {
                        "Two Factor Authentication",
                        "Two File Access",
                        "Second Firewall Access",
                        "Dual Internet"
                    },
                    CorrectAnswer=0,
                    Explanation="2FA adds an extra layer of security."
                }
            };
        }
    }
}