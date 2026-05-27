/*
    Erwin Mashobane
    ST10073464
*/

using System;
using System.Collections.Generic;
using System.Linq;

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
                { "password", new List<string> { "password", "passcode", "login", "credentials" } },
                { "phishing", new List<string> { "phishing", "fake email", "scam email" } },
                { "privacy", new List<string> { "privacy", "tracking", "permissions" } },
                { "malware", new List<string> { "malware", "virus", "trojan", "spyware" } },
                { "scam", new List<string> { "scam", "fraud", "impersonation" } },
                { "vpn", new List<string> { "vpn", "virtual private network" } },
                { "wifi", new List<string> { "wifi", "public wifi", "hotspot" } },
                { "2fa", new List<string> { "2fa", "two-factor authentication", "authentication" } },
                { "ransomware", new List<string> { "ransomware", "encrypted files" } },
                { "downloads", new List<string> { "downloads", "download files" } }
            };

            keywordResponses = new Dictionary<string, List<string>>
            {
                {
                    "password",
                    new List<string>
                    {
                        "🔒 Use strong passwords with symbols and numbers.",
                        "🛡️ Never reuse passwords across multiple websites.",
                        "💡 Use a password manager for secure storage."
                    }
                },

                {
                    "phishing",
                    new List<string>
                    {
                        "🎣 Never click suspicious email links.",
                        "⚠️ Check sender email addresses carefully.",
                        "📧 Banks never ask for passwords via email.",
                        "⚠️ Look carefully for spelling mistakes in phishing emails."
                    }
                },

                {
                    "privacy",
                    new List<string>
                    {
                        "🛡️ Review your social media privacy settings.",
                        "🔒 Limit personal information shared online.",
                        "📱 Disable unnecessary app permissions.",
                        "🛡️ Use secure websites that start with HTTPS."
                    }
                },

                {
                    "malware",
                    new List<string>
                    {
                        "💻 Install trusted antivirus software to prevent malware infections.",
                        "⚠️ Avoid downloading files from unknown sites.",
                        "🔒 Keep Windows updated for security patches.",
                        "⚠️ Malware can steal sensitive information from your device."
                    }
                },

                {
                    "scam",
                    new List<string>
                    {
                        "🚨 If it sounds too good to be true, it probably is.",
                        "💰 Never send money to strangers online.",
                        "📞 Ignore fake lottery or prize calls.",
                        "💰 Be careful of investment scams on social media."
                    }
                },

                {
                    "vpn",
                    new List<string>
                    {
                        "🌍 VPNs protect your internet traffic on public Wi-Fi.",
                        "🔒 A VPN helps keep your browsing private.",
                        "📡 Use trusted VPN providers only."
                    }
                },

                {
                    "wifi",
                    new List<string>
                    {
                        "🌍 Avoid logging into banking apps on public WiFi.",
                        "🔒 Use a VPN when connected to public hotspots.",
                        "🚨Hackers may monitor unsecured WiFi networks."
                    }
                },

                {
                    "2fa",
                    new List<string>
                    {
                        "🔒 Two-factor authentication adds extra account security.",
                        "💡 Enable 2FA on banking and email accounts.",
                        "🚨 Authenticator apps are safer than SMS codes."
                    }
                },

                {
                    "ransomware",
                    new List<string>
                    {
                        "💾 Backup important files regularly.",
                        "⚠️ Never open suspicious attachments.",
                        "🔒 Ransomware encrypts your files for payment."
                    }
                },

                {
                    "downloads",
                    new List<string>
                    {
                        "💾 Only download files from trusted websites.",
                        "💻 Scan downloads before opening them.",
                        "⚠️ Pirated software often contains malware."
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
                    if (input.Contains(alias))
                    {
                        LastMatchedKeyword = keyword.Key;

                        List<string> responses = keywordResponses[keyword.Key];
                        return responses[random.Next(responses.Count)];
                    }
                }
            }

            return "I don't have information on that topic. Try asking about password, phishing, privacy, malware, scam, vpn, wifi, 2fa, ransomware, or downloads.";
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