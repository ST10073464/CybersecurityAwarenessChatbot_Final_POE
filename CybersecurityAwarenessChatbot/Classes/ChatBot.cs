/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Services;

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
                "I'm not sure I understand yet. Try asking about passwords, scams, privacy, tasks or quizzes.",
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
            // Initialize response variable
            string response = string.Empty;

            // Maintain raw case variations if needed for history
            string rawInput = input;

            input = input.ToLower().Trim();


            // FIRST MESSAGE = USERNAME REGISTRATION
            if (awaitingName)
            {
                string formattedName = string.Join(" ",
                    rawInput.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));

                // Grab the persistent historical record before saving the new input
                string lastSavedUser = MemoryStore.LastLoggedInUser;

                memoryStore.RememberUserName(formattedName);
                awaitingName = false;

                ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} started chatbot");
                ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} logged in");

                // Returning User matching check
                if (!string.IsNullOrEmpty(lastSavedUser) &&
                    lastSavedUser.Equals(MemoryStore.UserName, StringComparison.OrdinalIgnoreCase) &&
                    MemoryStore.ConversationHistory.Count > 0)
                {
                    string previousChats = string.Join("\n", MemoryStore.ConversationHistory.TakeLast(10));

                    return $"👋 Welcome back, {MemoryStore.UserName}!\n\n" +
                           $"I remember you.\n\n" +
                           $"📝 Here are your recent chats:\n\n" +
                           $"{previousChats}\n\n" +
                           $"How can I assist you today?";
                }

                // New User profile greeting branch
                return $"😊 Nice to meet you, {MemoryStore.UserName}!\n\n" +
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

            // CONVERSATIONAL AUDIT LOGGING
            // Everything a verified user does must be captured in the main chat activity log
            ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} initiated chat");

            // Store the raw user input in the conversation history for context and future reference
            MemoryStore.ConversationHistory.Add($"User: {rawInput}");

            memoryStore.AddConversation($"User: {rawInput}");
            // ... process response ...
            memoryStore.AddConversation($"Bot: {response}");

            // INTENT DETECTION MATCH ROUTING (XAML Windows Redirection Strings)
            string intent = NLPService.DetectIntent(input);

            switch (intent)
            {
                case "ADD_TASK":
                case "VIEW_TASKS":
                case "DELETE_TASK":
                case "COMPLETE_TASK":
                case "VIEW_REMINDERS":
                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} opened task window");
                    MemoryStore.TaskWelcomeMessage =
                        "📋 TASK ASSISTANT\n\n" +
                        "Please select the option you would like to perform from the left panel ." +
                        "Or Type an Option:\n\n" +
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
                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} viewed activity logs");
                    return "__SHOW_ACTIVITY_LOG__";
            }

            // 4. SESSION ABORT / SYSTEM CONTROLS COMMAND INTERACTION
            if (input == "exit" || input == "sign out" || input == "logout" || input == "leave session")
            {
                ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} ended the session");
                awaitingName = true;
                LastMatchedKeyword = "";
                return "__LEAVE_SESSION__";
            }

            if (input == "end" || input == "close" || input == "leave")
            {
                ActivityLogService.Add("MAIN", $"{MemoryStore.UserName} closed SecureWin");
                return "__CLOSE_SECUREWIN__";
            }

            // 5. MEMORY EVALUATION MATCHES
            if (input.Contains("what is my name") || input.Contains("do you remember my name") || input.Contains("who am i"))
            {
                string nameReply = $"😊 Your name is {MemoryStore.UserName}.";
                MemoryStore.ConversationHistory.Add($"Bot: {nameReply}");
                return nameReply;
            }

            // 6. DYNAMIC CONVERSATION BREAK DOWN & FOLLOW-UP QUERY MANAGEMENT
            if (input.Contains("tell me more") || input.Contains("another tip") || input.Contains("explain more") || input.Contains("continue"))
            {
                string followUpText = keywordResponder.GetFollowUpResponse();
                string followUpResponse = !string.IsNullOrEmpty(LastMatchedKeyword)
                    ? $"\n{MemoryStore.UserName}, here is more about {LastMatchedKeyword}:\n\n{followUpText}"
                    : $"{MemoryStore.UserName}, {followUpText}";

                MemoryStore.ConversationHistory.Add($"Bot: {followUpResponse}");
                return followUpResponse;
            }

            // 7. FAVORITE CYBERSECURITY TOPIC STORAGE
            if (input.Contains("interested in"))
            {
                foreach (string keyword in keywordResponder.GetAllKeywords())
                {
                    if (input.Contains(keyword))
                    {
                        memoryStore.FavouriteTopic = keyword;
                        string interestResponse = $"\nGreat, {MemoryStore.UserName}! \n\nI'll remember that you're interested in {keyword}.";
                        MemoryStore.ConversationHistory.Add($"Bot: {interestResponse}");
                        return interestResponse;
                    }
                }
            }

            // 8. SENTIMENT PARSING AND KNOWLEDGE BASE RESPONSES
            Sentiment sentiment = sentimentDetector.Detect(input);
            string keywordResponse = keywordResponder.GetResponse(input);
            string sentimentResponse = sentimentDetector.GetSentimentResponse(MemoryStore.UserName, sentiment);
            string tipResponse = sentimentDetector.GetCybersecurityTip(MemoryStore.UserName, sentiment);

            // KEYWORD MATCH STRUCTURING WITH OPTIONAL SENTIMENT TIERS
            if (!string.IsNullOrEmpty(keywordResponse))
            {
                LastMatchedKeyword = keywordResponder.LastMatchedKeyword;
                response = $"{MemoryStore.UserName},\n\n{keywordResponse}";

                if (!string.IsNullOrEmpty(sentimentResponse)) response += $"\n\n{sentimentResponse}";
                if (!string.IsNullOrEmpty(tipResponse)) response += $"\n\n{tipResponse}";

                MemoryStore.ConversationHistory.Add($"Bot: {response}");
                return response;
            }

            // Sentiment-Only Fallback Engine
            if (sentiment != Sentiment.Neutral)
            {
                response = $"Detected Sentiment: {sentiment}\n\n{sentimentResponse}\n\n{tipResponse}";
                MemoryStore.ConversationHistory.Add($"Bot: {response}");
                return response;
            }

            // 9. CHATBOT STATIC QA ASSIGNMENT RULES
            if (input.Contains("how are you") || input.Contains("how are things") || input.Contains("are you okay"))
            {
                string res = $"😊 I'm functioning perfectly, {MemoryStore.UserName}, and ready to help keep you safe online!";
                MemoryStore.ConversationHistory.Add($"Bot: {res}");
                return res;
            }

            if (input.Contains("purpose") || input.Contains("what is your purpose") || input.Contains("why were you created"))
            {
                string res = $"🎯 My purpose is to educate users like you, {MemoryStore.UserName}, about cybersecurity awareness and online safety.";
                MemoryStore.ConversationHistory.Add($"Bot: {res}");
                return res;
            }

            if (input.Contains("what can you do") || input.Contains("help me with") || input.Contains("features"))
            {
                string res = $"{MemoryStore.UserName}, 💡 I can help with phishing, passwords, scams, malware, privacy, ransomware, VPNs, and online safety tips.";
                MemoryStore.ConversationHistory.Add($"Bot: {res}");
                return res;
            }

            if (input.Contains("who created you") || input.Contains("who made you"))
            {
                string res = $"👨‍💻 I was created by Erwin Mashobane to help users stay safe online.";
                MemoryStore.ConversationHistory.Add($"Bot: {res}");
                return res;
            }

            if (input.Contains("thank you") || input.Contains("thanks"))
            {
                string res = $"😊 You're welcome, {MemoryStore.UserName}! I'm always here to help.";
                MemoryStore.ConversationHistory.Add($"Bot: {res}");
                return res;
            }

            if (input.Contains("hello") || input.Contains("hi"))
            {
                string res = $"👋 Hello again, {MemoryStore.UserName}! How can I help you today?";
                MemoryStore.ConversationHistory.Add($"Bot: {res}");
                return res;
            }

            // 10. SYSTEM FALLBACK ENGINE
            string fallback = $"{MemoryStore.UserName},\n\n {fallbackResponses[random.Next(fallbackResponses.Count)]}";
            MemoryStore.ConversationHistory.Add($"Bot: {fallback}");
            return fallback;
        }
    }
}
