/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;

namespace CybersecurityAwarenessChatbot.Classes
{
    public class ChatBot
    {
        private readonly KeywordResponder keywordResponder;
        private readonly SentimentDetector sentimentDetector;
        private readonly MemoryStore memoryStore;

        private readonly Random random;

        private bool awaitingName = true;

        private readonly List<string> fallbackResponses;

        private string LastMatchedKeyword = "";

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

        public string GetGreeting()
        {
            return "👋 Welcome to SecureWin!\n\nWhat is your name?";
        }

        public string ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return "⚠️ Please enter a message.";
            }

            input = input.Trim();
            string lowerInput = input.ToLower();

            // NAME HANDLING

            if (awaitingName)
            {
                string previousName = memoryStore.UserName;

                memoryStore.RememberUserName(input);
                awaitingName = false;

                if (!string.IsNullOrEmpty(previousName) &&
                    previousName.Equals(memoryStore.UserName, StringComparison.OrdinalIgnoreCase) &&
                    memoryStore.ConversationHistory.Count > 0)
                {
                    return $"👋 Welcome back, {memoryStore.UserName}!\n\nHere are your previous chats:\n\n" +
                           string.Join("\n", memoryStore.ConversationHistory);
                }

                return $"😊 Nice to meet you, {memoryStore.UserName}!\n\nYou can ask me about:\n" +
                       $"🔒 Passwords\n🎣 Phishing\n🛡️ Privacy\n💻 Malware\n⚠️ Scams\n\nType 'exit' to leave.";
            }

            memoryStore.AddConversation($"User: {input}");

            // EXIT

            if (lowerInput == "exit" || lowerInput == "quit" || lowerInput == "bye")
            {
                awaitingName = true;
                LastMatchedKeyword = "";

                return $"👋 Goodbye, {memoryStore.UserName}. Stay safe online!";
            }

            // MEMORY

            if (lowerInput.Contains("what is my name") ||
                lowerInput.Contains("do you remember my name") ||
                lowerInput.Contains("who am i"))
                {
                    return $"😊 Your name is {memoryStore.UserName}, Erwin.";
                }

            // FOLLOW UP

            if (lowerInput.Contains("tell me more") ||
                lowerInput.Contains("another tip") ||
                lowerInput.Contains("explain more") ||
                lowerInput.Contains("continue"))
                {
                    if (!string.IsNullOrEmpty(LastMatchedKeyword))
                    {
                        return $"{memoryStore.UserName}, here is more about {LastMatchedKeyword}:\n" +
                           $"{keywordResponder.GetFollowUpResponse()}";
                     }

                return $"{memoryStore.UserName}, {keywordResponder.GetFollowUpResponse()}";
            }

            // KEYWORD RESPONSE

            string keywordResponse = keywordResponder.GetResponse(lowerInput);

            if (!string.IsNullOrEmpty(keywordResponse))
            {
                LastMatchedKeyword = keywordResponder.LastMatchedKeyword;

                string response =
                    $"{memoryStore.UserName}, {keywordResponse}\n\n" +
                    $"{sentimentDetector.GetCybersecurityTip(memoryStore.UserName, sentimentDetector.Detect(lowerInput))}";

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // SENTIMENT RESPONSE

            Sentiment sentiment = sentimentDetector.Detect(lowerInput);

            if (sentiment != Sentiment.Neutral)
            {
                string response =
                    $"{memoryStore.UserName}, {sentimentDetector.GetSentimentResponse(memoryStore.UserName, sentiment)}\n\n" +
                    $"{sentimentDetector.GetCybersecurityTip(memoryStore.UserName, sentiment)}";

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // FALLBACK

            string fallback =
                $"{memoryStore.UserName}, " +
                fallbackResponses[random.Next(fallbackResponses.Count)];

            memoryStore.AddConversation($"Bot: {fallback}");

            return fallback;
        }
    }
}