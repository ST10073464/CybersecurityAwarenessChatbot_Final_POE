/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    public static class ActivityLogService
    {
        private static readonly List<string> allLogs = new();

        public static void Add(string source, string action)
        {
            string log =
                $"[{DateTime.Now:HH:mm:ss}] [{source}] {action}";

            allLogs.Add(log);

            // Keep only latest 50 logs
            if (allLogs.Count > 50)
                allLogs.RemoveAt(0);
        }

        // Main Window -> all logs
        public static string GetAllLogs()
        {
            if (allLogs.Count == 0)
                return "No activities recorded.";

            return string.Join(
                "\n\n",
                allLogs.TakeLast(10));
        }

        // Window-specific logs
        public static string GetLogs(string source)
        {
            var logs = allLogs
                .Where(l => l.Contains($"[{source}]"))
                .TakeLast(10)
                .ToList();

            if (logs.Count == 0)
                return $"No {source} activities found.";

            return string.Join("\n\n", logs);
        }

        public static List<string> GetRecentActivities(int count = 10)
        {
            return allLogs
                .TakeLast(count)
                .Reverse()
                .ToList();
        }
    }
}