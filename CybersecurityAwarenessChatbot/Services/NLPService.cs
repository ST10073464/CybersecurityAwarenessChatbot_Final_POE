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
    }
}