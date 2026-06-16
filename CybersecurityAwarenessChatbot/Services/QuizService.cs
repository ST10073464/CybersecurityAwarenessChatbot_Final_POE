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
                    Question="Which password is strongest?",
                    Options=new List<string>
                    {
                        "password123",
                        "abc123",
                        "MyDog",
                        "G7#kP!92Lm$Q"
                    },
                    CorrectAnswer=3,
                    Explanation="Strong passwords use upper case, lower case, numbers and symbols."
                },

                new QuizQuestion
                {
                    Question="True or False: Public Wi-Fi is always safe.",
                    Options=new List<string>
                    {
                        "True",
                        "False"
                    },
                    CorrectAnswer=1,
                    Explanation="Public Wi-Fi can expose your data to attackers."
                },

                new QuizQuestion
                {
                    Question="What is phishing?",
                    Options=new List<string>
                    {
                        "A fishing website",
                        "A scam to steal information",
                        "Antivirus software",
                        "Firewall"
                    },
                    CorrectAnswer=1,
                    Explanation="Phishing tricks users into revealing sensitive information."
                },

                new QuizQuestion
                {
                    Question="What is malware?",
                    Options=new List<string>
                    {
                        "Harmful software",
                        "Antivirus",
                        "Browser",
                        "Search engine"
                    },
                    CorrectAnswer=0,
                    Explanation="Malware is software designed to damage devices or steal information."
                },

                new QuizQuestion
                {
                    Question="True or False: Software updates improve security.",
                    Options=new List<string>
                    {
                        "True",
                        "False"
                    },
                    CorrectAnswer=0,
                    Explanation="Updates often contain security patches."
                },

                new QuizQuestion
                {
                    Question="What is social engineering?",
                    Options=new List<string>
                    {
                        "Building websites",
                        "Manipulating people to reveal information",
                        "Coding",
                        "Installing software"
                    },
                    CorrectAnswer=1,
                    Explanation="Social engineering attacks target human behaviour rather than technology."
                },

                new QuizQuestion
                {
                    Question="What should you do before clicking a link?",
                    Options=new List<string>
                    {
                        "Click immediately",
                        "Check the URL",
                        "Share it",
                        "Ignore it"
                    },
                    CorrectAnswer=1,
                    Explanation="Always verify links before clicking them."
                },

                new QuizQuestion
                {
                    Question="True or False: Using the same password everywhere is safe.",
                    Options=new List<string>
                    {
                        "True",
                        "False"
                    },
                    CorrectAnswer=1,
                    Explanation="Using unique passwords protects multiple accounts."
                },

                new QuizQuestion
                {
                    Question="What should you do if your account is hacked?",
                    Options=new List<string>
                    {
                        "Do nothing",
                        "Change password immediately",
                        "Delete browser",
                        "Restart PC"
                    },
                    CorrectAnswer=1,
                    Explanation="Changing your password immediately helps secure your account."
                },

                new QuizQuestion
                {
                    Question="Which is an example of personal information?",
                    Options=new List<string>
                    {
                        "Username",
                        "ID Number",
                        "Browser",
                        "Search Engine"
                    },
                    CorrectAnswer=1,
                    Explanation="An ID number is sensitive personal information."
                },

                new QuizQuestion
                {
                    Question="True or False: Antivirus software helps protect against malware.",
                    Options=new List<string>
                    {
                        "True",
                        "False"
                    },
                    CorrectAnswer=0,
                    Explanation="Antivirus software helps detect and remove malware."
                },

                new QuizQuestion
                {
                    Question="What is the safest action when receiving an unexpected attachment?",
                    Options=new List<string>
                    {
                        "Open it",
                        "Download it",
                        "Verify the sender first",
                        "Forward it"
                    },
                    CorrectAnswer=2,
                    Explanation="Always verify the sender before opening attachments."
                }

            };

        }
    }
}