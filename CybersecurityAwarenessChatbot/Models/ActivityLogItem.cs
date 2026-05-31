using System;

namespace CybersecurityAwarenessChatbot.Models
{
    public class ActivityLogItem
    {
        public DateTime TimeStamp { get; set; }

        public string Action { get; set; }

        public override string ToString()
        {
            return $"{TimeStamp:g} - {Action}";
        }
    }
}