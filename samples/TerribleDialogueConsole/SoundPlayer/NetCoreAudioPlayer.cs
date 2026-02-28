using NetCoreAudio;

namespace TerribleDialogueConsole
{
    internal class NetCoreAudioPlayer : ISoundPlayer
    {
        private Player player = new Player();
        private string currentSoundPath;

        public void Play(string path)
        {
            currentSoundPath = path;
            player.Play(path);
        }

        public void Stop()
        {
            currentSoundPath = null;
            player.Stop();
        }

        public void SetVolume(float value)
        {
            player.SetVolume((byte)value);
        }

        public void PlayLooping(string path)
        {
            Play(path);

            player.PlaybackFinished += Player_PlaybackFinished;
        }

        private void Player_PlaybackFinished(object sender, EventArgs e)
        {
            player.PlaybackFinished -= Player_PlaybackFinished;

            PlayLooping(currentSoundPath);
        }

    }
}