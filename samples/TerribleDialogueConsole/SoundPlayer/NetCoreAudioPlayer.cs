using NetCoreAudio;

namespace TerribleDialogueConsole
{
    internal class NetCoreAdioPlayer : ISoundPlayer
    {
        private Player player = new Player();

        public void Play(string path) {
            player.Play(path);
        }

        public void Stop() {
            player.Stop();
        }

        public void SetVolume(float value) {
            player.SetVolume((byte)value);
        }
    }
}