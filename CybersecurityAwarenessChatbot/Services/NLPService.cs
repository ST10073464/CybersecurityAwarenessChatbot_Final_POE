/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    public static class NLPService
    {
        public static string DetectIntent(string input)
        {
            input = input.ToLower();

            // TASKS
            if (input.Contains("add task") ||
                input.Contains("create task") ||
                input.Contains("new task"))               
            {
                return "ADD_TASK";
            }

            // REMINDERS
            if (input.Contains("reminder") ||
                input.Contains("view reminders") ||
                input.Contains("show reminders"))
            {
                return "VIEW_REMINDERS";
            }

            // VIEW TASKS
            if (input.Contains("view tasks") ||
                input.Contains("show tasks") ||
                input.Contains("my tasks") ||
                input.Contains("task list"))
            {
                return "VIEW_TASKS";
            }

            // COMPLETE TASK
            if (input.Contains("complete task") ||
                input.Contains("finish task") ||
                input.Contains("done"))
            {
                return "COMPLETE_TASK";
            }

            // DELETE TASK
            if (input.Contains("delete task") ||
                input.Contains("remove task"))
            {
                return "DELETE_TASK";
            }

            // QUIZ
            if (input.Contains("quiz") ||
                input.Contains("game") ||
                input.Contains("test me") ||
                input.Contains("question"))
            {
                return "QUIZ";
            }

            // PASSWORD
            if (input.Contains("password"))
            {
                return "PASSWORD";
            }

            // PHISHING
            if (input.Contains("phishing") ||
                input.Contains("fake email"))
            {
                return "PHISHING";
            }

            // PRIVACY
            if (input.Contains("privacy") ||
                input.Contains("personal information"))
            {
                return "PRIVACY";
            }

            // MALWARE
            if (input.Contains("malware") ||
                input.Contains("virus"))
            {
                return "MALWARE";
            }

            // ACTIVITY LOG
            if (input.Contains("what have you done") ||
                input.Contains("activity") ||
                input.Contains("recent actions") ||
                input.Contains("history"))
            {
                return "ACTIVITY_LOG";
            }

            // LEAVE SESSION
            if (input.Contains("leave session") ||
                input.Contains("logout") ||
                input.Contains("sign out"))
            {
                return "LEAVE_SESSION";
            }

            // BACK to MainWindow
            if (input.Contains("exit") ||
                input.Contains("leave") ||
                input.Contains("main"))
            {
                return "EXIT";
            }

            return "UNKNOWN";
        }
    }
}