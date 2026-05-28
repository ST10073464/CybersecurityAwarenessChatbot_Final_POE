/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;

namespace CybersecurityAwarenessChatbot.Classes
{
    // Main chatbot engine.
    // Controls memory, sentiment, keyword recognition, and conversation flow.
    public class ChatBot
    {
        private readonly KeywordResponder keywordResponder;
        private readonly SentimentDetector sentimentDetector;
        private readonly MemoryStore memoryStore;

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
                "I'm not sure I understand yet. Try asking about passwords, scams, or privacy.",
                "Could you rephrase that? I can help with cybersecurity topics.",
                "Ask me about phishing, malware, passwords, or online safety."
            };
        }

        // Initial greeting message
        public string GetGreeting()
        {
            return "👋 Welcome to SecureWin!\n\nWhat is your name?";
        }

        // Main chatbot processing logic
        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "⚠️ Please enter a message.";
            }

            input = input.Trim();

            string lowerInput = input.ToLower();

            // CAPTURE USER NAME

            if (awaitingName)
            {
                string previousName = memoryStore.UserName;

                memoryStore.RememberUserName(input);

                awaitingName = false;

                // Returning user
                if (
                    !string.IsNullOrEmpty(previousName) &&
                    previousName.Equals(
                        memoryStore.UserName,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    memoryStore.ConversationHistory.Count > 0
                   )
                {
                    string previousChats =
                        string.Join(
                            "\n",
                            memoryStore.ConversationHistory
                        );

                    return $"👋 Welcome back, {memoryStore.UserName}!\n\n" +
                           $"Here are your previous chats:\n\n" +
                           $"{previousChats}";
                }

                return $"😊 Nice to meet you, {memoryStore.UserName}!\n\n" +
                       $"You can ask me about:\n\n" +
                       $"🔒 Passwords\n" +
                       $"🎣 Phishing\n" +
                       $"🛡️ Privacy\n" +
                       $"💻 Malware\n" +
                       $"⚠️ Scams\n\n" +
                       $"Type 'exit' anytime to leave the chat.";
            }

            // Save user message
            memoryStore.AddConversation(
                $"{memoryStore.UserName}: {input}"
            );

            // EXIT OPTIONS

            if (
                lowerInput == "exit" ||
                lowerInput == "quit" ||
                lowerInput == "bye"
               )
            {
                awaitingName = true;

                LastMatchedKeyword = "";

                return $"👋 Goodbye, {memoryStore.UserName}.\n\n" +
                       $"Stay safe online and come back anytime!";
            }

            // MEMORY QUESTIONS

            if (
                lowerInput.Contains("what is my name") ||
                lowerInput.Contains("do you remember my name") ||
                lowerInput.Contains("who am i")
               )
            {
                return $"😊 Your name is {memoryStore.UserName}.";
            }

            // FOLLOW-UP QUESTIONS

            if (
                lowerInput.Contains("tell me more") ||
                lowerInput.Contains("another tip") ||
                lowerInput.Contains("explain more") ||
                lowerInput.Contains("continue")
               )
            {
                return $"{memoryStore.UserName}, {keywordResponder.GetFollowUpResponse()}";
            }

            // STORE FAVOURITE TOPIC

            if (lowerInput.Contains("interested in"))
            {
                foreach (string keyword in keywordResponder.GetAllKeywords())
                {
                    if (lowerInput.Contains(keyword))
                    {
                        memoryStore.FavouriteTopic = keyword;

                        return $"😊 Great, {memoryStore.UserName}! " +
                               $"I'll remember that you're interested in {keyword}.";
                    }
                }
            }

            // SENTIMENT DETECTION

            Sentiment sentiment =
                sentimentDetector.Detect(lowerInput);

            string sentimentResponse =
                sentimentDetector.GetSentimentResponse(
                    memoryStore.UserName,
                    sentiment
                );

            string tipResponse =
                sentimentDetector.GetCybersecurityTip(
                    memoryStore.UserName,
                    sentiment
                );

            // SPECIAL QUESTIONS

            if (
                lowerInput.Contains("how are you") ||
                lowerInput.Contains("how are things") ||
                lowerInput.Contains("are you okay")
               )
            {
                return $"😊 I'm functioning perfectly, {memoryStore.UserName}, and ready to help keep you safe online!";
            }

            if (
                lowerInput.Contains("purpose") ||
                lowerInput.Contains("what is your purpose") ||
                lowerInput.Contains("why were you created")
               )
            {
                return $"🎯 My purpose is to educate users like you, {memoryStore.UserName}, about cybersecurity awareness and online safety.";
            }

            if (
                lowerInput.Contains("what can you do") ||
                lowerInput.Contains("help me with") ||
                lowerInput.Contains("features")
               )
            {
                return $"💡 {memoryStore.UserName}, I can help with phishing, passwords, scams, malware, privacy, ransomware, VPNs, and online safety tips.";
            }

            if (
                lowerInput.Contains("who created you") ||
                lowerInput.Contains("who made you")
               )
            {
                return $"👨‍💻 I was created by Erwin Mashobane to help users like you, {memoryStore.UserName}, stay safe online.";
            }

            if (
                lowerInput.Contains("thank you") ||
                lowerInput.Contains("thanks")
               )
            {
                return $"😊 You're welcome, {memoryStore.UserName}! I'm always here to help.";
            }

            if (
                lowerInput.Contains("hello") ||
                lowerInput.Contains("hi")
               )
            {
                return $"👋 Hello again, {memoryStore.UserName}! How can I help you today?";
            }

            // KEYWORD RESPONSES

            string keywordResponse =
                keywordResponder.GetResponse(lowerInput);

            if (!string.IsNullOrEmpty(keywordResponse))
            {
                LastMatchedKeyword =
                    keywordResponder.LastMatchedKeyword;

                string response =
                    $"{sentimentResponse}\n\n" +
                    $"{keywordResponse}\n\n" +
                    $"{tipResponse}";

                memoryStore.AddConversation(
                    $"Bot: {response}"
                );

                return response;
            }

            // SENTIMENT ONLY RESPONSE

            if (sentiment != Sentiment.Neutral)
            {
                string response = $"{sentimentResponse}\n\n{tipResponse}";

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // PERSONALISED FALLBACK

            string fallback = $"{memoryStore.UserName}, " +
                fallbackResponses[random.Next(fallbackResponses.Count)];

            memoryStore.AddConversation($"Bot: {fallback}");

            return fallback;
        }
    }
}
