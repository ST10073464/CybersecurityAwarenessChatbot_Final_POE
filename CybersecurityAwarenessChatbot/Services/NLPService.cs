namespace CybersecurityAwarenessChatbot.Classes
{
    public class NLPService
    {
        public string DetectIntent(string input)
        {
            input = input.ToLower();

            if (input.Contains("add task") ||
                input.Contains("create task"))
                return "ADD_TASK";

            if (input.Contains("remind"))
                return "REMINDER";

            if (input.Contains("quiz"))
                return "QUIZ";

            if (input.Contains("activity") ||
                input.Contains("what have you done"))
                return "LOG";

            return "CHAT";
        }

        public string DetectTaskIntent(string input)
        {
            input = input.ToLower();

            if (input.Contains("add task") || input.StartsWith("task") || input.Contains("create task"))
                return "ADD_TASK";

            if (input.Contains("remind me"))
                return "SET_REMINDER";

            if (input.Contains("show tasks") || input.Contains("view tasks"))
                return "VIEW_TASKS";

            if (input.Contains("delete task"))
                return "DELETE_TASK";

            if (input.Contains("complete task"))
                return "COMPLETE_TASK";

            return "CHAT";
        }
    }
}