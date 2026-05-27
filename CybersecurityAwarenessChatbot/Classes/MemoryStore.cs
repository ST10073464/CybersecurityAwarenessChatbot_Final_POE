/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    // Stores chatbot memory and user preferences.
    public class MemoryStore
    {
        // User's name.
        public string UserName { get; set; } = "";

        // User's favourite cybersecurity topic.
        public string FavouriteTopic { get; set; } = "";

        // Stores the previous topic.
        public string LastTopic { get; set; } = "";

        // Stores conversation history.
        public List<string> ConversationHistory { get; set; }

        // Constructor
        public MemoryStore()
        {
            ConversationHistory = new List<string>();
        }

        // Save username with first letter uppercase
        public void RememberUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                // Remove extra spaces
                name = name.Trim().ToLower();

                // Make first letter uppercase
                UserName =
                    char.ToUpper(name[0]) +
                    name.Substring(1);
            }
        }

        // Check if chatbot remembers name
        public bool HasUserName()
        {
            return !string.IsNullOrWhiteSpace(UserName);
        }

        // Return remembered name
        public string RecallUserName()
        {
            if (HasUserName())
            {
                return $"😊 Your name is {UserName}.";
            }

            return "❌ I do not know your name yet.";
        }

        // Adds chat history.
        public void AddConversation(string message)
        {
            ConversationHistory.Add(message);
        }

        // Returns a personalised sentence.
        public string GetPersonalisedMessage()
        {
            if (!string.IsNullOrEmpty(FavouriteTopic))
            {
                return $"🔐 {UserName}, as someone interested in {FavouriteTopic}, you should stay informed about online threats.";
            }

            return "";
        }
    }
}