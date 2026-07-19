using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View.Input
{
    internal class KeybindConsoleInputHandler : IInputHandler<ConsoleKeyInfo>
    {
        public bool Intercept { get; }
        public ConsoleKeybind[] Keybinds { get; }

        public KeybindConsoleInputHandler(bool intercept, ConsoleKeybind[] keybinds)
        {
            Intercept = intercept;
            Keybinds = keybinds;
        }

        public bool TryGetInput(out ConsoleKeyInfo input)
        {
            input = Console.ReadKey(Intercept);
            return !TriggerKeybinds(input);
        }

        /// <summary>
        /// Trigger all keybind actions matching the <paramref name="keyInfo"/>
        /// </summary>
        /// <param name="keyInfo"></param>
        /// <returns>If a keybind has been triggered</returns>
        private bool TriggerKeybinds(ConsoleKeyInfo keyInfo)
        {
            foreach(ConsoleKeybind keybind in Keybinds)
            {
                if(keyInfo.Key == keybind.Key && keyInfo.Modifiers == keybind.Modifiers)
                {
                    keybind.Action.Invoke();
                    return true;
                }
            }

            return false;
        }
    }
}
