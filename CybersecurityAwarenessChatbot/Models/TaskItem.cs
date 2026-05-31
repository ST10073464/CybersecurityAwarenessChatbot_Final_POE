/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Models
{
    // Represents a cybersecurity task
    public class TaskItem
    {
        // Unique task ID
        public int Id { get; set; }

        // Task title
        public string Title { get; set; } = "";

        // Task description
        public string Description { get; set; } = "";

        // Optional reminder date
        public DateTime? ReminderDate { get; set; }

        // Completion status
        public bool IsCompleted { get; set; }

        // Display task nicely inside ListBox
        public override string ToString()
        {
            string reminder =
                ReminderDate.HasValue
                ? ReminderDate.Value.ToShortDateString()
                : "No Reminder";

            return $"{Title} | {Description} | {reminder} | Completed: {IsCompleted}";
        }
    }
}