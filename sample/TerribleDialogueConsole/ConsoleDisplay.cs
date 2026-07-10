using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole
{
    internal static class ConsoleDisplay
    {
        private const string CHOICE_DISPLAY_CHARS = "1234567890abcdefghijklmnopqrstuvwxyz";

        public static int Menu(object[] options) => Menu(options, []);

        public static int Menu(object[] options, Keybind[] keybinds) => Menu(options, o => o.ToString(), keybinds);

        public static int Menu(object[] options, Func<object, string> displayFunc, Keybind[] keybinds)
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            // Can't support more than what we display
            ArraySegment<object> displayableChoices = new ArraySegment<object>(options, 0, Math.Min(CHOICE_DISPLAY_CHARS.Length, options.Length));
            for(int i = 0; i < displayableChoices.Count; i++)
            {
                char displayChar = CHOICE_DISPLAY_CHARS[i];
                Console.WriteLine($"{displayChar}. {displayFunc(options[i])}");
            }

            int choiceIndex = -1;
            while(choiceIndex < 0 || choiceIndex >= displayableChoices.Count)
            {
                if(!TryReadKey(true, keybinds, out ConsoleKeyInfo keyInfo))
                    return -1;

                char choice = keyInfo.KeyChar;
                choiceIndex = CHOICE_DISPLAY_CHARS.IndexOf(choice);
            }

            for(int i = displayableChoices.Count - 1; i >= 0 && Console.CursorTop > 0; i--)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            }

            Console.WriteLine($"> {displayFunc(options[choiceIndex])}");
            Console.ResetColor();
            return choiceIndex;
        }

        /// <summary>
        /// Read key with support for additional actions using keybinds
        /// </summary>
        /// <param name="intercept"></param>
        /// <param name="keybinds"></param>
        /// <param name="consoleKeyInfo"></param>
        /// <returns>false if a keybind has been used, true otherwise</returns>
        public static bool TryReadKey(bool intercept, Keybind[] keybinds, out ConsoleKeyInfo consoleKeyInfo)
        {
            consoleKeyInfo = Console.ReadKey(intercept);
            foreach(Keybind keybind in keybinds)
            {
                if(consoleKeyInfo.Key == keybind.Key && consoleKeyInfo.Modifiers == keybind.Modifiers)
                {
                    keybind.Action.Invoke();
                    return false;
                }
            }

            return true;
        }
    }
}
