namespace CybersecurityAwarenessChatbot.Classes
{
    public class ActivityLogService
    {
        private readonly List<ActivityLogItem> logs = new();

        public void AddLog(string action)
        {
            logs.Add(new ActivityLogItem
            {
                Timestamp = DateTime.Now,
                Action = action
            });
        }

        public List<ActivityLogItem> GetRecentLogs()
        {
            return logs
                .OrderByDescending(x => x.Timestamp)
                .Take(10)
                .ToList();
        }
    }
}