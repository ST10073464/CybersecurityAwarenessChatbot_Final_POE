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

        private readonly DatabaseService databaseService;

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

        public string ProcessInput(string input)
        {
            input = input.ToLower().Trim();

            // CAPTURE USER NAME
            if (awaitingName)
            {
                string previousName = MemoryStore.UserName;

                memoryStore.RememberUserName(input);
                awaitingName = false;

                if (
                        !string.IsNullOrEmpty(previousName) &&
                        previousName.Equals(MemoryStore.UserName, StringComparison.OrdinalIgnoreCase) &&
                        MemoryStore.ConversationHistory.Count > 0
                    )

                {
                    string previousChats =
                    string.Join("\n", MemoryStore.ConversationHistory);

                    return $"\n👋 Welcome back, {MemoryStore.UserName}!\n\n" +
                           $"\nHere are your previous chats:\n\n{previousChats}";
                }

                return $"😊 Nice to meet you, {MemoryStore.UserName}!\n\n" +
                       $"I am here to help you stay safe online:\n\n" +
                       $"you can ask about CYBESECURITY related questions like:\n\n" +
                       $"🔒 Passwords, " + $"🎣 Phishing\n\n" +
                       $"You can 📋 Add Task, " + $"📋 Play Quiz, " + $"📋 View your activity\n\n" +

                       $"Tell me what you want to do, and I will gladly assit you.";
               
            }

            memoryStore.AddConversation($"User: {input}");

            string intent = NLPService.DetectIntent(input);

            switch (intent)
            {
                case "ADD_TASK":

                    ActivityLogService.Add("TASK",
                        $"{MemoryStore.UserName} requested to add a task");

                    MemoryStore.TaskWelcomeMessage =
                        "📋 Add Task\n\n" +
                        "Please enter the title and description of the task you would like to add.";

                    return "__OPEN_TASK_ADD__";


                case "VIEW_TASKS":

                    if (!databaseService.GetTasks().Any())
                    {
                        return "⚠ No tasks have been added yet.\n\n" +
                               "Would you like to add a new task?\n\n" +
                               "Type: Add Task";
                    }

                    ActivityLogService.Add("TASK",
                        $"{MemoryStore.UserName} requested to view tasks");

                    MemoryStore.TaskWelcomeMessage =
                        "📋 Here are your current tasks.";

                    return "__OPEN_TASK_VIEW__";


                case "DELETE_TASK":

                    if (!databaseService.GetTasks().Any())
                    {
                        return "⚠ There are no tasks available to delete.\n\n" +
                               "Would you like to add a task instead?\n\n" +
                               "Type: Add Task";
                    }

                    ActivityLogService.Add("TASK",
                        $"{MemoryStore.UserName} requested to delete a task");

                    MemoryStore.TaskWelcomeMessage =
                        "🗑 Enter the title of the task you would like to delete.";

                    return "__OPEN_TASK_DELETE__";


                case "COMPLETE_TASK":

                    if (!databaseService.GetTasks().Any())
                    {
                        return "⚠ No tasks exist to complete.\n\n" +
                               "Would you like to add a task first?\n\n" +
                               "Type: Add Task";
                    }

                    ActivityLogService.Add(
                        "TASK",
                        $"{MemoryStore.UserName} requested to complete a task");

                    MemoryStore.TaskWelcomeMessage =
                        "✅ Enter the title of the task you have completed.";

                    return "__OPEN_TASK_COMPLETE__";


                case "QUIZ":

                    ActivityLogService.Add("TASK", $"{MemoryStore.UserName} started the quiz");

                    return "__OPEN_QUIZ__";


                case "ACTIVITY_LOG":

                    ActivityLogService.Add("TASK",
                        $"{MemoryStore.UserName} viewed activity logs");

                    return "__SHOW_ACTIVITY_LOG__";
            }

            if (input.Equals("leave session") ||
                input.Equals("logout") ||
                input.Equals("sign out") ||
                input.Equals("exit"))
            {
                ActivityLogService.Add("TASK",
                    $"{MemoryStore.UserName} left the session");

                return "__LEAVE_SESSION__";
            }

            /* // EXIT OPTIONS
             if (input == "exit" || input == "quit" || input == "bye")
             {
                 awaitingName = true;
                 LastMatchedKeyword = "";

                 return "__CLEAR_CHAT__\n\n👋 Chat ended successfully.\n\nWelcome back!\n\nWhat is your name?";
             }

             // END → close the whole application
             if (input == "end" || input == "close" || input == "leave")
             {
                 Application.Current.Shutdown();
                 return null;
             }
            */
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
