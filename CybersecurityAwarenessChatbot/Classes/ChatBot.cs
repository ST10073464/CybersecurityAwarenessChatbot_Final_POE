/*
    Erwin Mashobane
    ST10073464
*/
using CybersecurityAwarenessChatbot.Services;
using System.Windows;

namespace CybersecurityAwarenessChatbot.Classes
{
    // Main chatbot engine.
    // Controls memory, sentiment, keyword recognition, and conversation flow.
    public class ChatBot
    {
        private readonly KeywordResponder keywordResponder;
        private readonly SentimentDetector sentimentDetector;
        private readonly MemoryStore memoryStore;

        private readonly DatabaseService databaseService = new DatabaseService();

        private readonly Random random;

        private bool awaitingName = true;

        private readonly List<string> fallbackResponses;

        private string LastMatchedKeyword = "";

        // Constructor
        public ChatBot()
        {
            keywordResponder = new KeywordResponder();
            sentimentDetector = new SentimentDetector();
            memoryStore = new MemoryStore();

            random = new Random();

            fallbackResponses = new List<string>
            {
                "I'm not sure I understand yet. Try asking about passwords, scams, privacy, tasks or quizes.",
                "Could you rephrase that? I can help with cybersecurity topics.",
                "Ask me about phishing, malware, passwords, or online safety."
            };
        }

        public string GetGreeting()
        {
            return "👋 Welcome to SecureWin!\n\n" +
                   "Please type your name to continue.";
        }

        public void ResetConversation()
        {
            awaitingName = true;
        }

        public void ResetSession()
        {
            awaitingName = true;

            LastMatchedKeyword = "";

        }

        public string ProcessInput(string input)
        { 

            input = input.ToLower().Trim();

            // FIRST MESSAGE = USERNAME
            if (awaitingName)
            {
                string formattedName = string.Join(" ",
                    input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(word =>
                            char.ToUpper(word[0]) +
                                word.Substring(1).ToLower()));

                string previousName = MemoryStore.UserName;

                memoryStore.RememberUserName(formattedName);

                awaitingName = false;

                ActivityLogService.Add(
                    "MAIN",
                    $"{MemoryStore.UserName} logged in");

                // Returning user
                if (
                        !string.IsNullOrEmpty(previousName) &&
                        previousName.Equals(MemoryStore.UserName, StringComparison.OrdinalIgnoreCase) &&
                        MemoryStore.ConversationHistory.Count > 0
                    )
                {
                    string previousChats =
                    string.Join("\n", MemoryStore.ConversationHistory.TakeLast(10));

                    return
                        $"👋 Welcome back, {MemoryStore.UserName}!\n\n" +
                        $"I remember you.\n\n" +
                        $"📝 Here are your recent chats:\n\n" +
                        $"{previousChats}\n\n" +
                        $"How can I assist you today?";
                }

                // New user
                return
                    $"😊 Nice to meet you, {MemoryStore.UserName}!\n\n" +
                    $"I am here to help you stay safe online.\n\n" +
                    $"You can:\n\n" +
                    $"🔒 Password Safety\n" +
                    $"🎣 Phishing Awareness\n" +
                    $"📋 Task Management\n" +
                    $"📝 Activity Logs\n" +
                    $"🧠 Cybersecurity Quiz\n\n" +
                    $"Tell me what you would like to do.\n\n" +
                    $"Type 'exit' anytime to leave the chat.";
            }

            //memoryStore.AddConversation($"User: {input}");
            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} started logged in");

            string intent = NLPService.DetectIntent(input);

            switch (intent)
            {
                case "ADD_TASK":

                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened task window");

                    MemoryStore.TaskWelcomeMessage =
                        "📋 TASK ASSISTANT\n\n" +
                        "Please select the option you would like to perform from the left panel.\n\n" +
                        "✔ Add Task\n" +
                        "✔ View Tasks\n" +
                        "✔ Complete Task\n" +
                        "✔ Delete Task\n" +
                        "✔ View Reminders\n" +
                        "✔ View Saved JSON Tasks";

                    return "__OPEN_TASK_WINDOW__";

                case "VIEW_TASKS":

                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened task window");

                    MemoryStore.TaskWelcomeMessage =
                        "📋 TASK ASSISTANT\n\n" +
                        "Please select the option you would like to perform from the left panel.\n\n" +
                        "✔ Add Task\n" +
                        "✔ View Tasks\n" +
                        "✔ Complete Task\n" +
                        "✔ Delete Task\n" +
                        "✔ View Reminders\n" +
                        "✔ View Saved JSON Tasks";

                    return "__OPEN_TASK_WINDOW__";

                case "DELETE_TASK":

                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened task window");

                    MemoryStore.TaskWelcomeMessage =
                         "📋 TASK ASSISTANT\n\n" +
                         "Please select the option you would like to perform from the left panel.\n\n" +
                         "✔ Add Task\n" +
                         "✔ View Tasks\n" +
                         "✔ Complete Task\n" +
                         "✔ Delete Task\n" +
                         "✔ View Reminders\n" +
                         "✔ View Saved JSON Tasks";

                    return "__OPEN_TASK_WINDOW__";

                case "COMPLETE_TASK":

                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened task window");

                    MemoryStore.TaskWelcomeMessage =
                         "📋 TASK ASSISTANT\n\n" +
                         "Please select the option you would like to perform from the left panel.\n\n" +
                         "✔ Add Task\n" +
                         "✔ View Tasks\n" +
                         "✔ Complete Task\n" +
                         "✔ Delete Task\n" +
                         "✔ View Reminders\n" +
                         "✔ View Saved JSON Tasks";

                    return "__OPEN_TASK_WINDOW__";

                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened task window");

                case "VIEW_REMINDERS":
                    MemoryStore.TaskWelcomeMessage =
                        "📋 TASK ASSISTANT\n\n" +
                        "Please select the option you would like to perform from the left panel.\n\n" +
                        "✔ Add Task\n" +
                        "✔ View Tasks\n" +
                        "✔ Complete Task\n" +
                        "✔ Delete Task\n" +
                        "✔ View Reminders\n" +
                        "✔ View Saved JSON Tasks";

                    return "__OPEN_TASK_WINDOW__";

                case "QUIZ":

                    ActivityLogService.Add("QUIZ", $"{MemoryStore.UserName} started the quiz");

                    return "__OPEN_QUIZ__";

                case "ACTIVITY_LOG":

                    ActivityLogService.Add(
                        "TASK",
                        $"{MemoryStore.UserName} viewed activity logs");

                    return "__SHOW_ACTIVITY_LOG__";
            }
            // LEAVE SESSION → allow new user
            if (input == "exit" ||
                input == "sign out" ||
                input == "logout" ||
                input == "leave session")
            {
                ActivityLogService.Add(
                    "MAIN",
                    $"{MemoryStore.UserName} ended the session");

                awaitingName = true;

                LastMatchedKeyword = "";

                return "__LEAVE_SESSION__";
            }

            // CLOSE APPLICATION
            if (input == "end" ||
                input == "close" ||
                input == "leave")
            {
                ActivityLogService.Add(
                    "MAIN",
                    $"{MemoryStore.UserName} closed SecureWin");

                return "__CLOSE_SECUREWIN__";
            }

            // MEMORY QUESTIONS
            if (input.Contains("what is my name") ||
                input.Contains("do you remember my name") ||
                input.Contains("who am i"))
            {
                return $"😊 Your name is {MemoryStore.UserName}.";
            }

            // FOLLOW-UP QUESTIONS
            if (input.Contains("tell me more") ||
                input.Contains("another tip") ||
                input.Contains("explain more") ||
                input.Contains("continue"))
            {
                if (!string.IsNullOrEmpty(LastMatchedKeyword))
                {
                    return $"\n{MemoryStore.UserName}, here is more about {LastMatchedKeyword}:\n\n" +
                           $"\n{keywordResponder.GetFollowUpResponse()}";
                }

                return $"{MemoryStore.UserName}, {keywordResponder.GetFollowUpResponse()}";
            }

            // STORE FAVOURITE TOPIC
            if (input.Contains("interested in"))
            {
                foreach (string keyword in keywordResponder.GetAllKeywords())
                {
                    if (input.Contains(keyword))
                    {
                        memoryStore.FavouriteTopic = keyword;

                        return $"\nGreat, {MemoryStore.UserName}! \n\n" +
                               $"\nI'll remember that you're interested in {keyword}.";
                    }
                }
            }

            // SENTIMENT DETECTION
            Sentiment sentiment =
                sentimentDetector.Detect(input);

            string keywordResponse =
                keywordResponder.GetResponse(input);

            string sentimentResponse =
                sentimentDetector.GetSentimentResponse(MemoryStore.UserName, sentiment);

            string tipResponse =
                sentimentDetector.GetCybersecurityTip(MemoryStore.UserName, sentiment);

            // KEYWORD RESPONSES (FIXED PRIORITY)

            if (!string.IsNullOrEmpty(keywordResponse))
            {
                LastMatchedKeyword = keywordResponder.LastMatchedKeyword;

                string response = $"{MemoryStore.UserName},\n\n{keywordResponse}";

                // Only add sentiment if it exists
                if (!string.IsNullOrEmpty(sentimentResponse))
                {
                    response += $"\n\n{sentimentResponse}";
                }

                // Only add tip if it exists
                if (!string.IsNullOrEmpty(tipResponse))
                {
                    response += $"\n\n{tipResponse}";
                }

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // Sentiment only response
            if (sentiment != Sentiment.Neutral)
            {
                string response =
                    $"Detected Sentiment: {sentiment}\n\n" +
                    $"{sentimentResponse}\n\n" +
                    $"{tipResponse}";

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // SPECIAL QUESTIONS
            if (input.Contains("how are you") ||
                input.Contains("how are things") ||
                input.Contains("are you okay"))
            {
                return $"😊 I'm functioning perfectly, {MemoryStore.UserName}, and ready to help keep you safe online!";
            }

            if (input.Contains("purpose") ||
                input.Contains("what is your purpose") ||
                input.Contains("why were you created"))
            {
                return $"🎯 My purpose is to educate users like you, {MemoryStore.UserName}, about cybersecurity awareness and online safety.";
            }

            if (input.Contains("what can you do") ||
                input.Contains("help me with") ||
                input.Contains("features"))
            {
                return $"{MemoryStore.UserName}, 💡 I can help with phishing, passwords, scams, malware, privacy, ransomware, VPNs, and online safety tips.";
            }

            if (input.Contains("who created you") ||
                input.Contains("who made you"))
            {
                return $"👨‍💻 I was created by Erwin Mashobane to help users stay safe online.";
            }

            if (input.Contains("thank you") ||
                input.Contains("thanks"))
            {
                return $"😊 You're welcome, {MemoryStore.UserName}! I'm always here to help.";
            }

            if (input.Contains("hello") ||
                input.Contains("hi"))
            {
                return $"👋 Hello again, {MemoryStore.UserName}! How can I help you today?";
            }

            // PERSONALISED FALLBACK
            string fallback =
                $"{MemoryStore.UserName},\n\n " +
                fallbackResponses[random.Next(fallbackResponses.Count)];

            memoryStore.AddConversation($"Bot: {fallback}");

            return fallback;
        }
    }
}
