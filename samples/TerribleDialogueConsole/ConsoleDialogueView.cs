using TerribleDialogue;
using System;
using System.Collections.Generic;
using System.Text;
using TerribleDialogue.Data;
using TerribleDialogueConsole.View;

namespace TerribleDialogue
{
    internal class ConsoleDialogueView : IDialogueView
    {
        private const string CHOICE_DISPLAY_CHARS = "1234567890abcdefghijklmnopqrstuvwxyz";
        private const string LINE_BREAK = "<br>";

        public void DisplayLine(LineData line)
        {
            string displayType = line.Tags.GetValueOrDefault("display", "newline");
            string block = line.Tags.GetValueOrDefault("block", "yes");
            string[] splitLines = line.Text.Split(LINE_BREAK);

            Console.ForegroundColor = ColorByName(line.Tags.GetValueOrDefault("color", "white"));

            foreach(string linePart in splitLines)
            {
                Console.Write(linePart);
                while(block == "yes" && Console.ReadKey(true).Key != ConsoleKey.Enter) { } // Read enter but swallow it
            }

            Console.ResetColor();
            
            if(displayType == "newline")
                Console.WriteLine();
        }

        public int DisplayChoices(string[] choices)
        {
            // Can't support more than what we display
            ArraySegment<string> displayableChoices = new ArraySegment<string>(choices, 0, Math.Min(CHOICE_DISPLAY_CHARS.Length, choices.Length));
            for(int i = 0; i < displayableChoices.Count; i++)
            {
                char displayChar = CHOICE_DISPLAY_CHARS[i];
                Console.WriteLine($"{displayChar}. {choices[i]}");
            }

            int choiceIndex = -1;
            while(choiceIndex < 0 || choiceIndex >= displayableChoices.Count)
            {
                char choice = Console.ReadKey(true).KeyChar;
                choiceIndex = CHOICE_DISPLAY_CHARS.IndexOf(choice);
            }

            for(int i = displayableChoices.Count - 1; i >= 0 && Console.CursorTop > 0; i--)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            }

            Console.WriteLine($"> {choices[choiceIndex]}");
            return choiceIndex;
        }

        private static ConsoleColor ColorByName(string name)
        {
            if(Enum.TryParse(name, true, out ConsoleColor color))
            {
                return color;
            } else
            {
                return ConsoleColor.White;
            }
        }
    }
}
