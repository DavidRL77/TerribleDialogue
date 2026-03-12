using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerribleDialogueConsole.SoundPlayer;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Loading libvlc takes a while
            Console.WriteLine("Loading LibVLC...");
            using(var player = new LibVLCAudioPlayer())
            {
                App p = new App(player);
                p.Run();
            }
        }
    }
}
