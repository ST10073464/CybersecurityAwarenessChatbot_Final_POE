/*
    Erwin Mashobane
    ST10073464
*/

using CybersecurityAwarenessChatbot.Classes;

namespace CybersecurityAwarenessChatbot.Classes
{
    // Possible user emotions.
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated,
        Happy,
        Angry
    }

    // Detects user sentiment and returns empathetic responses.
    public class SentimentDetector
    {
        private readonly Dictionary<Sentiment, List<string>> triggerWords;
        private readonly Dictionary<Sentiment, List<string>> sentimentResponses;

        private readonly Random random;

        public SentimentDetector()
        {
            random = new Random();

            // Trigger words for each sentiment
            triggerWords = new Dictionary<Sentiment, List<string>>
            {
                {
                    Sentiment.Worried,
                    new List<string>
                    {
                        "worried",
                        "scared",
                        "afraid",
                        "unsafe",
                        "nervous",
                        "anxious"
                    }
                },

                {
                    Sentiment.Curious,
                    new List<string>
                    {
                        "curious",
                        "interested",
                        "wondering",
                        "learn"
                    }
                },

                {
                    Sentiment.Frustrated,
                    new List<string>
                    {
                        "frustrated",
                        "confused",
                        "annoyed",
                        "irritated"
                    }
                },

                {
                    Sentiment.Happy,
                    new List<string>
                    {
                        "great",
                        "awesome",
                        "thanks",
                        "happy"
                    }
                },

                {
                    Sentiment.Angry,
                    new List<string>
                    {
                        "angry",
                        "mad",
                        "upset",
                        "furious"
                    }
                }
            };

            // Empathetic responses
            sentimentResponses = new Dictionary<Sentiment, List<string>>
            {
                {
                    Sentiment.Worried,
                    new List<string>
                    {
                        "😟 It's understandable to feel worried about online threats.",
                        "⚠️ Cybersecurity can feel overwhelming, but staying informed helps.",
                        "🤔 You're not alone — many people worry about scams online."
                    }
                },

                {
                    Sentiment.Curious,
                    new List<string>
                    {
                        "😊 That's great curiosity helps improve cybersecurity awareness.",
                        "💡 Learning about online safety is a smart step.",
                        "🔐 Cybersecurity knowledge is very valuable today."
                    }
                },

                {
                    Sentiment.Frustrated,
                    new List<string>
                    {
                        "🤔 I understand this topic can feel frustrating.",
                        "⚠️ Cybersecurity can be confusing at first.",
                        "😊 Don't worry — I'll explain it clearly."
                    }
                },

                {
                    Sentiment.Happy,
                    new List<string>
                    {
                        "😊 I'm glad you're enjoying learning about cybersecurity.",
                        "🎉 That's great to hear.",
                        "👍 Awesome — let's continue improving your online safety."
                    }
                },

                {
                    Sentiment.Angry,
                    new List<string>
                    {
                        "😟 I understand your frustration.",
                        "😤 Online threats can be very upsetting.",
                        "🤝 Let's work through this together calmly."
                    }
                }
            };
        }

        // Detects the user's sentiment
        // Detects the user's sentiment
        public Sentiment Detect(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Sentiment.Neutral;
            }

            input = input.ToLower();

            // Remove punctuation
            char[] punctuation =
            {
                '.', ',', '!', '?', ';', ':', '"', '\''
            };

            foreach (char character in punctuation)
            {
                input = input.Replace(character.ToString(), "");
            }

            // DEBUG TEST
            Console.WriteLine("INPUT: " + input);

            // Check trigger words
            foreach (var sentiment in triggerWords)
            {
                foreach (string triggerWord in sentiment.Value)
                {
                    Console.WriteLine("CHECKING: " + triggerWord);

                    if (input.Contains(triggerWord))
                    {
                        Console.WriteLine("MATCH FOUND: " + triggerWord);

                        return sentiment.Key;
                    }
                }
            }

            return Sentiment.Neutral;
        }
        // Returns empathetic response
        // Returns empathetic response with username
        public string GetSentimentResponse(
            string username,
            Sentiment sentiment
        )
        {
            if (sentiment == Sentiment.Neutral)
            {
                return "";
            }

            // Make first letter uppercase
            username =
                char.ToUpper(username[0]) +
                username.Substring(1).ToLower();

            List<string> responses =
                sentimentResponses[sentiment];

            string selectedResponse =
                responses[random.Next(responses.Count)];

            // Insert username into response
            return string.Format(selectedResponse, username);
        }

        // Returns cybersecurity tips
        // Returns cybersecurity tips with username
        public string GetCybersecurityTip(string username, Sentiment sentiment)
        {
            // Make first letter uppercase
            username =
                char.ToUpper(username[0]) +
                username.Substring(1).ToLower();

            switch (sentiment)
            {
                case Sentiment.Worried:
                    return $"🔐 Tip for you, {username}: Never click suspicious links or emails from unknown senders.";

                case Sentiment.Curious:
                    return $"💡 Tip for you, {username}: Keep learning about phishing, scams, and password safety.";

                case Sentiment.Frustrated:
                    return $"⚠️ Tip for you, {username}: Use strong passwords and enable two-factor authentication.";

                case Sentiment.Happy:
                    return $"✅ Great job, {username}. Continue practicing safe browsing habits online.";

                case Sentiment.Angry:
                    return $"🛡️ Tip for you, {username}: Report suspicious websites and online scams immediately.";

                default:
                    return "";
            }
        }
    }
}