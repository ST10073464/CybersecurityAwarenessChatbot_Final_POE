namespace CybersecurityAwarenessChatbot.Classes
{
    public class ActivityLogItem
    {
        public DateTime Timestamp { get; set; }

        public string Action { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm}] {Action}";
        }
    }
}