/*
    Erwin Mashobane
    ST10073464
*/

/*Save data to a physical file on the hard drive.

Using a lightweight JSON file (Data/session.json) to save and reload the state dynamically.*/

using System.IO;
using System.Text.Json;

namespace CybersecurityAwarenessChatbot.Classes
{
    public class MemoryStore
    {
        public static string UserName { get; set; } = "";
        public static string LastLoggedInUser { get; set; } = "";
        public string FavouriteTopic { get; set; } = "";

        // Store history mapped directly by individual username keys
        public static Dictionary<string, List<string>> UserHistories { get; set; } = new();

        // Helper property to safely extract the active user's chat logs
        public static List<string> ConversationHistory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(UserName)) return new List<string>();
                if (!UserHistories.ContainsKey(UserName)) UserHistories[UserName] = new List<string>();
                return UserHistories[UserName];
            }
        }

        public string LastTopic { get; set; } = "";
        public static string TaskWelcomeMessage { get; set; } = "";

        private static readonly string SessionFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "session.json");

        public MemoryStore() { }

        public void RememberUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                UserName = string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));

                LastLoggedInUser = UserName;

                // Ensure an isolated entry list exists for this user immediately upon login
                if (!UserHistories.ContainsKey(UserName))
                {
                    UserHistories[UserName] = new List<string>();
                }

                SaveSession();
            }
        }

        // Writes data map directly to JSON layout
        public static void SaveSession()
        {
            try
            {
                var sessionData = new SessionDataModel
                {
                    LastLoggedInUser = LastLoggedInUser,
                    UserHistories = UserHistories ?? new Dictionary<string, List<string>>()
                };

                string json = JsonSerializer.Serialize(sessionData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SessionFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
            }
        }

        // Loads multi-user map from disk on start
        public static void LoadSession()
        {
            try
            {
                if (File.Exists(SessionFilePath))
                {
                    string json = File.ReadAllText(SessionFilePath);
                    var sessionData = JsonSerializer.Deserialize<SessionDataModel>(json);
                    if (sessionData != null)
                    {
                        LastLoggedInUser = sessionData.LastLoggedInUser ?? "";
                        UserHistories = sessionData.UserHistories ?? new Dictionary<string, List<string>>();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load failed: {ex.Message}");
            }
        }

        // Adds chat histories to the targeted logged-in user profile container only
        public void AddConversation(string message)
        {
            if (string.IsNullOrWhiteSpace(UserName) || UserName == "Guest") return;

            if (!UserHistories.ContainsKey(UserName))
            {
                UserHistories[UserName] = new List<string>();
            }

            UserHistories[UserName].Add(message);
            SaveSession();
        }

        public bool HasUserName() => !string.IsNullOrWhiteSpace(UserName);
    }

    // JSON file container layout matching parameters
    public class SessionDataModel
    {
        public string LastLoggedInUser { get; set; } = "";
        public Dictionary<string, List<string>> UserHistories { get; set; } = new();
    }
}