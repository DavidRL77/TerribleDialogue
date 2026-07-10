using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole
{
    internal class Keybind
    {
        public ConsoleKey Key { get; }
        public ConsoleModifiers Modifiers { get; }
        public Action Action { get; }

        public Keybind(ConsoleKey key, ConsoleModifiers modifiers, Action action)
        {
            Key = key;
            Modifiers = modifiers;
            Action = action;
        }

    }
}
