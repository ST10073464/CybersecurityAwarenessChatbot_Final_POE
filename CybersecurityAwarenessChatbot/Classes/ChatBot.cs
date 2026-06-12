/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    // Main chatbot engine.
    // Controls memory, sentiment, keyword recognition, and conversation flow.
    public class ChatBot
    {
        private readonly KeywordResponder keywordResponder;
        private readonly SentimentDetector sentimentDetector;
        private readonly MemoryStore memoryStore;

        // Random generator for fallback responses
        private readonly Random random;

        private readonly List<string> fallbackResponses;

        // Stores the last matched keyword for follow-up questions
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

        // Greeting with usename entered in the landing page
        public string GetGreeting()
        {
            return $"👋 Welcome {MemoryStore.UserName} to the Cybersecurity Chatbot Assistant!\n\n" +
                   $"💬 Chat Mode Activated\n\n" +
                   $"You can ask me anything about cybersecurity.";
        }

        // Process user messages and generate responses based on keywords, sentiment, and memory
        public string ProcessInput(string input)
        {
            input = input.ToLower().Trim();

            // Save user input to memory
            memoryStore.AddConversation($"User: {input}");

            // Returns to the Landing Page and clears the chat history
            if (input == "exit" || input == "quit" || input == "bye")
            {
                return "__CLEAR_CHAT__\n\n👋 Chat session ended.\n\nReturning to menu...";
            }

            // Memory-based responses about the user's name or identity, using stored information for a personalised touch
            if (input.Contains("what is my name") ||
                input.Contains("do you remember my name") ||
                input.Contains("who am i"))
            {
                return $"😊 Your name is {MemoryStore.UserName}.";
            }

            // Follow -up questions about the last topic discussed, using the stored keyword for context
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

            // Store user's favourite topic based on keywords for personalised responses later
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

            // Sentiment analysis to detect user emotions and adjust responses accordingly, providing empathy and support based on the detected sentiment
            Sentiment sentiment =
                sentimentDetector.Detect(input);
            
            string keywordResponse =
                keywordResponder.GetResponse(input);

            string sentimentResponse =
                sentimentDetector.GetSentimentResponse(MemoryStore.UserName, sentiment);

            string tipResponse =
                sentimentDetector.GetCybersecurityTip(MemoryStore.UserName, sentiment);

            // Keyword response with optional sentiment and tip. Only include sentiment and tip if they exist to avoid empty sections in the response.

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

            // Special questions
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
                return $"💡 I can help with:\n\n" + 
                        "• Password Security\n" + 
                        "• Phishing Detection\n" + 
                        "• Online Scams\n" + 
                        "• Malware\n" + 
                        "• Privacy Settings\n" + 
                        "• Ransomware\n" + 
                        "• VPN Security\n" + 
                        "• Safe Browsing";
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