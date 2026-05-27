/*
    Erwin Mashobane
    ST10073464
*/

using System;
using System.Collections.Generic;

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
            input = input.ToLower().Trim();

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

                    return $"👋 Welcome back, {memoryStore.UserName}!\n" +
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
            memoryStore.AddConversation($"User: {input}");

            // EXIT OPTIONS

            if (
                input == "exit" ||
                input == "quit" ||
                input == "bye"
               )
            {
                awaitingName = true;

                LastMatchedKeyword = "";

                return $"👋 Chat ended successfully.\n\n" +
                       $"Welcome back!\n" +
                       $"What is your name?";
            }

            // MEMORY QUESTIONS

            if (
                input.Contains("what is my name") ||
                input.Contains("do you remember my name") ||
                input.Contains("who am i")
               )
            {
                return $"😊 Your name is {memoryStore.UserName}.";
            }

            // FOLLOW-UP QUESTIONS

            if (
                input.Contains("tell me more") ||
                input.Contains("another tip") ||
                input.Contains("explain more") ||
                input.Contains("continue")
               )
            {
                return $"{memoryStore.UserName},\n" +
                       $"{keywordResponder.GetFollowUpResponse()}";
            }

            // STORE FAVOURITE TOPIC

            if (input.Contains("interested in"))
            {
                foreach (string keyword in keywordResponder.GetAllKeywords())
                {
                    if (input.Contains(keyword))
                    {
                        memoryStore.FavouriteTopic = keyword;

                        return $"Great, {memoryStore.UserName}! " +
                               $"I'll remember that you're interested in {keyword}.";
                    }
                }
            }

            // SENTIMENT DETECTION

            Sentiment sentiment =
                sentimentDetector.Detect(input);

            string sentimentResponse =
                sentimentDetector.GetSentimentResponse(sentiment);

            string tipResponse =
                sentimentDetector.GetCybersecurityTip(sentiment);

            // TEST OUTPUT
            if (sentiment != Sentiment.Neutral)
            {
                return
                    $"Detected Sentiment: {sentiment}\n" +
                    sentimentResponse + "\n" +
                    tipResponse;
            }

            // SPECIAL QUESTIONS

            if (
                input.Contains("how are you") ||
                input.Contains("how are things") ||
                input.Contains("are you okay")
               )
            {
                return $"😊 I'm functioning perfectly, {memoryStore.UserName}, and ready to help keep you safe online!";
            }

            if (
                input.Contains("purpose") ||
                input.Contains("what is your purpose") ||
                input.Contains("why were you created")
               )
            {
                return $"🎯 My purpose is to educate users like you, {memoryStore.UserName}, about cybersecurity awareness and online safety.";
            }

            if (
                input.Contains("what can you do") ||
                input.Contains("help me with") ||
                input.Contains("features")
               )
            {
                return $"💡 {memoryStore.UserName}, I can help with phishing, passwords, scams, malware, privacy, ransomware, VPNs, and online safety tips.";
            }

            if (
                input.Contains("who created you") ||
                input.Contains("who made you")
               )
            {
                return $"👨‍💻 I was created by Erwin Mashobane to help users stay safe online.";
            }

            if (
                input.Contains("thank you") ||
                input.Contains("thanks")
               )
            {
                return $"😊 You're welcome, {memoryStore.UserName}! I'm always here to help.";
            }

            if (
                input.Contains("hello") ||
                input.Contains("hi")
               )
            {
                return $"👋 Hello again, {memoryStore.UserName}! How can I help you today?";
            }

            // KEYWORD RESPONSES

            string keywordResponse =
                keywordResponder.GetResponse(input);

            if (!string.IsNullOrEmpty(keywordResponse))
            {
                LastMatchedKeyword =
                    keywordResponder.LastMatchedKeyword;

                string response =
                    $"{memoryStore.UserName}," +
                    $"{sentimentResponse}" +
                    $"{keywordResponse}" +
                    $"{tipResponse}";

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // SENTIMENT ONLY RESPONSE

            if (sentiment != Sentiment.Neutral)
            {
                string response =
                    $"{memoryStore.UserName}," +
                    $"{sentimentResponse}" +
                    $"{keywordResponse}" +
                    $"{tipResponse}";

                memoryStore.AddConversation($"Bot: {response}");

                return response;
            }

            // PERSONALISED FALLBACK

            string fallback =
                $"{memoryStore.UserName}, " +
                fallbackResponses[random.Next(fallbackResponses.Count)];

            memoryStore.AddConversation($"Bot: {fallback}");

            return fallback;
        }
    }
}