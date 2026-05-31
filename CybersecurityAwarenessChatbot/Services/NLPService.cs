namespace CybersecurityAwarenessChatbot.Classes
{
    public class NLPService
    {
        public string DetectIntent(string input)
        {
            input = input.ToLower();

            if (input.Contains("add task"))
                return "ADD_TASK";

            if (input.Contains("remind me"))
                return "REMINDER";

            if (input.Contains("quiz"))
                return "QUIZ";

            if (input.Contains("activity log"))
                return "LOG";

            if (input.Contains("what have you done"))
                return "LOG";

            return "CHAT";
        }
    }
}