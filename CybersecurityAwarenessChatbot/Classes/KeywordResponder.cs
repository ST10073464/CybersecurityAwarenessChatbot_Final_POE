/*
    Erwin Mashobane
    ST10073464
*/

namespace CybersecurityAwarenessChatbot.Classes
{
    /// Handles cybersecurity keyword recognition and random responses.
    public class KeywordResponder
    {
        private readonly Dictionary<string, List<string>> keywordResponses;
        private readonly Dictionary<string, List<string>> keywordAliases;
        private readonly Random random;
        public string LastMatchedKeyword { get; private set; } = string.Empty;

        public KeywordResponder()
        {
            random = new Random();

            keywordAliases = new Dictionary<string, List<string>>
            {
                {"password",new List<string>{"password", "passwords", "passcode","login", "credentials", "signin", "sign in"}},
                {"phishing",new List<string>{"phishing", "fake email", "scam email","fake website", "phishing link", "email scam"}},
                {"privacy", new List<string>{"privacy", "tracking", "permissions","personal data", "online privacy", "data collection"}},
                {"malware", new List<string>{"malware", "virus", "trojan","spyware", "infected", "malicious software" }},
                {"scam",new List<string>{"scam", "scams", "fraud", "impersonation","fake call", "money scam", "online scam"}},
                {"vpn",new List<string>{"vpn", "virtual private network","secure connection", "encrypted browsing"}},
                {"wifi",new List<string>{"wifi", "wi-fi", "public wifi","hotspot", "wireless network"}},
                {"2fa",new List<string>{"2fa", "two-factor authentication","authentication", "multi-factor authentication","verification code", "authenticator"}},
                {"ransomware",new List<string>{"ransomware", "encrypted files","locked files", "ransom attack"}},
                {"download",new List<string>{"download","downloads", "download files","unsafe download", "file download","attachments"}}
            };

            keywordResponses = new Dictionary<string, List<string>>
            {
                {
                    "password",
                    new List<string>
                    {
                    "🔒 Use strong passwords with symbols and numbers.",
                    "🛡️ Never reuse passwords across multiple websites.",
                    "💡 Use a password manager for secure storage.",
                    "⚠️ Avoid sharing your passwords with anyone.",
                    "🔑 Change weak passwords immediately."
                    }
                },

                {
                    "phishing",
                    new List<string>
                    {
                        "🎣 Never click suspicious email links.",
                        "⚠️ Check sender email addresses carefully.",
                        "📧 Banks never ask for passwords via email.",
                        "⚠️ Look carefully for spelling mistakes in phishing emails.",
                        "🔍 Verify websites before entering login details."
                    }
                },

                {
                    "privacy",
                    new List<string>
                    {
                        "🛡️ Review your social media privacy settings.",
                        "🔒 Limit personal information shared online.",
                        "📱 Disable unnecessary app permissions.",
                        "🛡️ Use secure websites that start with HTTPS.",
                        "👤 Be careful about what you post publicly online."
                    }
                },

                {
                    "malware",
                    new List<string>
                    {
                        "💻 Install trusted antivirus software to prevent malware infections.",
                        "⚠️ Avoid downloading files from unknown sites.",
                        "🔒 Keep Windows updated for security patches.",
                        "⚠️ Malware can steal sensitive information from your device.",
                        "🛡️ Scan USB devices before opening files."
                    }
                },

                {
                    "scam",
                    new List<string>
                    {
                        "🚨 If it sounds too good to be true, it probably is.",
                        "💰 Never send money to strangers online.",
                        "📞 Ignore fake lottery or prize calls.",
                        "💰 Be careful of investment scams on social media.",
                        "⚠️ Scammers often create urgency to pressure victims."
                    }
                },

                {
                    "vpn",
                    new List<string>
                    {
                        "🌍 VPNs protect your internet traffic on public Wi-Fi.",
                        "🔒 A VPN helps keep your browsing private.",
                        "📡 Use trusted VPN providers only.",
                        "🛡️ VPNs help hide your online activity from attackers.",
                        "🌐 Always enable your VPN on public networks."
                    }
                },

                {
                    "wifi",
                    new List<string>
                    {
                        "🌍 Avoid logging into banking apps on public WiFi.",
                        "🔒 Use a VPN when connected to public hotspots.",
                        "🚨 Hackers may monitor unsecured WiFi networks.",
                        "📡 Disable auto-connect on unknown WiFi networks.",
                        "🛡️ Use password-protected WiFi whenever possible."
                    }
                },

                {
                    "2fa",
                    new List<string>
                    {
                        "🔒 Two-factor authentication adds extra account security.",
                        "💡 Enable 2FA on banking and email accounts.",
                        "🚨 Authenticator apps are safer than SMS codes.",
                        "📱 2FA helps protect accounts even if passwords are stolen.",
                        "🛡️ Enable multi-factor authentication wherever possible."
                    }
                },

                    {
                    "ransomware",
                    new List<string>
                    {
                        "💾 Backup important files regularly.",
                        "⚠️ Never open suspicious attachments.",
                        "🔒 Ransomware encrypts your files for payment.",
                        "🛡️ Keep backups stored separately from your computer.",
                        "🚨 Do not download software from untrusted websites."
                    }
                },

                {
                    "download",
                    new List<string>
                    {
                        "💾 Only download files from trusted websites.",
                        "💻 Scan downloads before opening them.",
                        "⚠️ Pirated software often contains malware.",
                        "🔒 Avoid downloading unknown email attachments.",
                        "🛡️ Keep your browser updated for safer downloads."
                    }
                }
            };
        }

        // Returns a random response for matched keyword.
        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please ask a cybersecurity question.";

            input = input.ToLowerInvariant();

            foreach (var keyword in keywordAliases)
            {
                foreach (string alias in keyword.Value)
                {
                    if (input.Contains(alias.ToLower()))
                    {
                        LastMatchedKeyword = keyword.Key;

                        // SAFE CHECK (PREVENT FALLBACK ERROR)

                        if (keywordResponses.ContainsKey(keyword.Key))
                        {
                            List<string> responses = keywordResponses[keyword.Key];
                            return responses[random.Next(responses.Count)];
                        }
                        else
                        {
                            return $"Here is information about {keyword.Key}. Stay safe online, {input}.";
                        }
                    }
                }
            }

            // ONLY runs if NOTHING matched

            return "";
        }

        // Returns another random response for follow-up questions.
        public string GetFollowUpResponse()
        {
            if (string.IsNullOrEmpty(LastMatchedKeyword))
                return "Please ask about a cybersecurity topic first.";

            List<string> responses = keywordResponses[LastMatchedKeyword];

            return responses[random.Next(responses.Count)];
        }

        // Returns all supported keywords.
        public List<string> GetAllKeywords()
        {
            return keywordResponses.Keys.ToList();
        }
    }
}