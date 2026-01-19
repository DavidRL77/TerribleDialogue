namespace TerribleDialogueConsole
{
    // Just a wrapper to make sure the program compiles on every platform
    public static class SoundPlayer
    {
#if WINDOWS
        private static System.Media.SoundPlayer soundPlayer = new System.Media.SoundPlayer();
        public static void Play(string path) {
            soundPlayer.SoundLocation = path;
            soundPlayer.Play();
        }

        public static void PlayLooping(string path) {
            soundPlayer.SoundLocation = path;
            soundPlayer.PlayLooping();
        }

        public static void Stop() {
            soundPlayer.Stop();
        }
#else
        public static void Play(string path)
        {
        }

        public static void PlayLooping(string path)
        {
        }

        public static void Stop()
        {
        }
#endif
    }
}