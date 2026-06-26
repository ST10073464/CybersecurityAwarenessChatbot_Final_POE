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
                // PHISHING (Multiple Choice) 
                new QuizQuestion
                {
                    Question = "What is phishing?",
                    Options = new List<string> 
                    { 
                        "A fishing website", 
                        "A scam to trick you into stealing information", 
                        "An antivirus software tool", 
                        "A firewall configuration" 
                    },
                    
                    CorrectAnswer = 1,
                    Explanation = "Phishing attacks use deceptive emails and messages to trick users into revealing sensitive credentials or information."
                },
                new QuizQuestion
                {
                    Question = "Which of the following is a classic example of a phishing attempt?",
                    Options = new List<string> 
                    { 
                        "Receiving an unexpected email asking you to verify your banking password via a link", 
                        "An automatic update prompt from your operating system", 
                        "Backing up your photos to a secure cloud drive", 
                        "Changing your social media password voluntarily" 
                    },
                    
                    CorrectAnswer = 0,
                    Explanation = "Phishing attempts frequently impersonate trusted institutions like banks to panic you into sharing passwords."
                },

                // PASSWORD SAFETY (True/False & Multiple Choice) 
                new QuizQuestion
                {
                    Question = "True or False: Using the same strong password across multiple accounts is safe.",
                    Options = new List<string> 
                    { 
                        "True", 
                        "False" 
                    },
                    
                    CorrectAnswer = 1,
                    Explanation = "If one account is compromised, attackers will use credential stuffing to access your other accounts. Use unique passwords."
                },
                new QuizQuestion
                {
                    Question = "Which password exhibits the strongest security structure?",
                    Options = new List<string> 
                    { 
                        "password123", 
                        "abc123", 
                        "MyDog", 
                        "G7#kP!92Lm$Q" 
                    },
                    
                    CorrectAnswer = 3,
                    Explanation = "Strong passwords use a combination of uppercase letters, lowercase letters, numbers, and special symbols."
                },

                // SAFE BROWSING - HTTPS & PUBLIC WI-FI (Multiple Choice) 
                new QuizQuestion
                {
                    Question = "What should you look for in the browser address bar to confirm a website connection is secure?",
                    Options = new List<string> 
                    { 
                        "An HTTP:// prefix", 
                        "An HTTPS:// prefix and a closed padlock icon", 
                        "Bright colors and advertisements", 
                        "A verified search engine button" 
                    },
                    
                    CorrectAnswer = 1,
                    Explanation = "HTTPS encrypts the data channel between your browser and the website, marked by the padlock icon."
                },
                new QuizQuestion
                {
                    Question = "What is the primary risk associated with entering banking details while connected to public Wi-Fi?",
                    Options = new List<string>
                    { 
                        "Your device storage will fill up quickly", 
                        "Attackers on the same network can intercept unencrypted data", 
                        "The website will load much faster than normal", 
                        "Your browser will automatically delete its data cache" 
                    },
                    
                    CorrectAnswer = 1,
                    Explanation = "Public Wi-Fi networks are often unsecured, enabling malicious actors to perform man-in-the-middle attacks to read traffic."
                },

                // SOCIAL ENGINEERING (True/False) 
                new QuizQuestion
                {
                    Question = "True or False: Social engineering attacks primarily target human behavior rather than technical system vulnerabilities.",
                    Options = new List<string> 
                    { 
                        "True", 
                        "False" 
                    },

                    CorrectAnswer = 0,
                    Explanation = "Social engineering manipulates human emotions like fear, trust, or urgency to trick victims into giving up access."
                },
                new QuizQuestion
                {
                    Question = "True or False: Social engineering tactics can only be executed through computer-based emails.",
                    Options = new List<string> 
                    { 
                        "True", 
                        "False" 
                    },

                    CorrectAnswer = 1,
                    Explanation = "Social engineering can occur via phone calls (vishing), SMS text messages (smishing), or in person."
                },

                // TWO-FACTOR AUTHENTICATION (Multiple Choice) 
                new QuizQuestion
                {
                    Question = "What is Two-Factor Authentication (2FA)?",
                    Options = new List<string> 
                    { 
                        "Using two different usernames on the same site", 
                        "Logging in twice consecutively", 
                        "Requiring a secondary verification step (like an OTP) in addition to your password", 
                        "Changing passwords every single day" 
                    },

                    CorrectAnswer = 2,
                    Explanation = "2FA adds an extra defense layer, making it much harder for hackers to access accounts even if they know the password."
                },

                // MALWARE AND RANSOMEWARE (True/False) 
                new QuizQuestion
                {
                    Question = "True or False: Antivirus software helps protect your operating system against malware installations.",
                    Options = new List<string> 
                    { 
                        "True", 
                        "False" 
                    },

                    CorrectAnswer = 0,
                    Explanation = "Antivirus software actively monitors file systems to detect, isolate, and remove malicious programs."
                },
                new QuizQuestion
                {
                    Question = "True or False: Regular data backups help recover your business operations from a ransomware attack.",
                    Options = new List<string> 
                    { 
                        "True", 
                        "False" 
                    },

                    CorrectAnswer = 0,
                    Explanation = "If ransomware encrypts your live environment files, separate offline backups allow you to restore systems without paying a ransom."
                },

                // PRIVACY SETTINGS (Multiple Choice) 
                new QuizQuestion
                {
                    Question = "Why should you regularly audit and restrict privacy settings on your social media accounts?",
                    Options = new List<string> 
                    { 
                        "To accelerate asset rendering speeds", 
                        "To limit what personal information is scraped or viewed by unknown third parties", 
                        "To maximize ad tracking options", 
                        "To decrease local device storage footprints" 
                    },
                    
                    CorrectAnswer = 1,
                    Explanation = "Restricting visibility settings stops malicious threat actors from gathering personal information to build target profiles for phishing."
                },

                //  DATA BACKUP (Multiple Choice) 
                new QuizQuestion
                {
                    Question = "What is considered a best practice for backing up critical digital information securely?",
                    Options = new List<string> 
                    { 
                        "Avoid running background file backup tools entirely", 
                        "Store all your file copies on the same local physical hard drive", 
                        "Regularly duplicate data to isolated, secure off-site or cloud target endpoints", 
                        "Run a complete archival profile backup only once a year" 
                    },
                    CorrectAnswer = 2,
                    Explanation = "Storing file copies dynamically in multiple locations (including off-site/cloud spaces) shields records from unexpected equipment failures or localized malware outbreaks."
                },

                //  VIRUS ATTACKS (Multiple Choice) 
                new QuizQuestion
                {
                    Question = "What is malware?",
                    Options = new List<string> 
                    { 
                        "Harmful software designed to compromise or steal data", 
                        "A safe browser configuration", 
                        "An internet tracking engine tool", 
                        "A hardware component extension" 
                    },
                    
                    CorrectAnswer = 0,
                    Explanation = "Malware includes viruses, spyware, and trojans intentionally constructed to disrupt devices and harvest corporate info."
                },
                new QuizQuestion
                {
                    Question = "What is ransomware?",
                    Options = new List<string> 
                    { 
                        "A digital web marketing search engine format", 
                        "Malicious software that encrypts user data and demands money for the recovery key", 
                        "A safe alternative text browser tool", 
                        "An active firewall network filtering application"
                    },
                    
                    CorrectAnswer = 1,
                    Explanation = "Ransomware locks down files using cryptographic algorithms, withholding key access until financial extortion requirements are satisfied."
                }
            };
        }
    }
}