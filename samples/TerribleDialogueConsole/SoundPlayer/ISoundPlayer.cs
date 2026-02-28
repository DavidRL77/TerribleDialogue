namespace TerribleDialogueConsole
{
    internal interface ISoundPlayer
    {
        public void Play(string path);
        public void PlayLooping(string path);
        public void Stop();
        public void SetVolume(float value);
    }
}