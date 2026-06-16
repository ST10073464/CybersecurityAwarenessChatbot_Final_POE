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
                input.Contains("new task") ||
                input.Contains("task"))
            {
                return "ADD_TASK";
            }

            // REMINDERS
            if (input.Contains("remind") ||
                input.Contains("reminder") ||
                input.Contains("notify"))
            {
                return "REMINDER";
            }

            // QUIZ
            if (input.Contains("quiz") ||
                input.Contains("game") ||
                input.Contains("test me") ||
                input.Contains("question"))
            {
                return "QUIZ";
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

            // BACK to chatwindow
            if (input.Contains("back") ||
                input.Contains("go back") ||
                input.Contains("done") ||
                input.Contains("finished") ||
                input.Contains("return"))
            {
                return "BACK";
            }

            return "UNKNOWN";
        }
    }
}