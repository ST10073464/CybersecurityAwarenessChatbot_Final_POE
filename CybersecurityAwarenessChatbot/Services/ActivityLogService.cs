/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    // Simple in-memory activity log service to track user actions
    public static class ActivityLogService
    {
        public static List<string> Actions = new();

        public static void Add(string action)
        {
            Actions.Add(action);
        }

        public static string GetSummary()
        {
            if (Actions.Count == 0)
            {
                return "No recent actions.";
            }

            string result =
                "📋 Here’s a summary of recent actions:\n\n";

            for (int i = 0; i < Actions.Count; i++)
            {
                result += $"{i + 1}. {Actions[i]}\n";
            }

            return result;
        }
    }
}