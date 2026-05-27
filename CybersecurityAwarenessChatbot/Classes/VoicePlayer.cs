/*
    Erwin Mashobane
    ST10073464
*/

using System.Media;

namespace CybersecurityAwarenessChatbot.Classes
{
    // Handles all chatbot sounds.
    public static class VoicePlayer
    {
        public static void PlayGreeting()
        {
            SoundPlayer player = new SoundPlayer("Audio/hello.wav");
            player.Play();
        }

        public static void PlaySound()
        {
            SoundPlayer player = new SoundPlayer("Audio/notification.wav");
            player.Play();
        }

    }
}