/*
    Erwin Mashobane
    ST10073464
*/

using System.IO;

namespace CybersecurityAwarenessChatbot.Services
{
    public static class ActivityLogService
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "activity_log.txt");

        // Your existing Add method remains the same...
        public static void Add(string category, string message)
        {
            try
            {
                string directory = Path.GetDirectoryName(LogFilePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{category.ToUpper()}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logLine);
            }
            catch { }
        }

        // Returns absolutely everything
        public static string GetAllLogs()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    return File.ReadAllText(LogFilePath);
                }
            }
            catch { }
            return "No logs available.";
        }

        // CRITICAL FIX: Returns logs matching only a specific category
        public static string GetLogsByCategory(string category)
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    var lines = File.ReadAllLines(LogFilePath);
                    // Filters lines containing matching tags like "[TASK]" or "[QUIZ]"
                    var filteredLines = lines.Where(line => line.Contains($"[{category.ToUpper()}]"));
                    return string.Join(Environment.NewLine, filteredLines);
                }
            }
            catch { }
            return $"No logs available for {category}.";
        }
    }
}