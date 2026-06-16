/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    public class QuizQuestion
    {
        public string Question { get; set; }

        public List<string> Options { get; set; }

        public int CorrectAnswer { get; set; }

        public string Explanation { get; set; }

        public void ShuffleOptions()
        {
            string correct =  Options[CorrectAnswer];

            Random rnd = new Random();

            Options = Options
                .OrderBy(x => rnd.Next())
                .ToList();

            CorrectAnswer = Options.IndexOf(correct);
        }
    }
}