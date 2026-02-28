using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.SoundPlayer
{
    internal class LibVLCAudioPlayer : ISoundPlayer, IDisposable
    {
        private readonly LibVLC libVLC;
        private readonly MediaPlayer player;

        public LibVLCAudioPlayer()
        {
            libVLC = new LibVLC();
            player = new MediaPlayer(libVLC);

            libVLC.Log += (obj, e) => { }; // Disable stderr output
        }

        public void Play(string path)
        {
            Stop();
            player.Media?.Dispose();

            player.Media = new Media(libVLC, new Uri(path));
            player.Play();
        }

        public void PlayLooping(string path)
        {
            Play(path);
            player.Media.AddOption(":input-repeat=999"); // Basically infinite
        }

        public void SetVolume(float value)
        {
            player.Volume = (int)value;
        }

        public void Stop()
        {
            player.Stop();
        }

        public void Dispose()
        {
            Stop();

            player.Media?.Dispose();
            player.Dispose();
            libVLC.Dispose();
        }
    }
}
