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

            //libVLC.Log += (obj, e) => { }; // Disable stderr output
        }

        public void Play(string path)
        {
            PlayInternal(new Media(libVLC, new Uri(path)));
        }

        public void PlayLooping(string path)
        {
            PlayInternal(new Media(libVLC, new Uri(path), ":input-repeat=9999"));
        }

        private void PlayInternal(Media media)
        {
            Stop();
            player.Media?.Dispose();

            player.Media = media;
            player.Play();
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
