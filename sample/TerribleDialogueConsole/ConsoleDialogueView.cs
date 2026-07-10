using TerribleDialogue.Data;
using TerribleDialogueConsole;
using TerribleDialogueConsole.View;

namespace TerribleDialogue
{
    internal class ConsoleDialogueView : IDialogueView
    {
        private const string LINE_BREAK = "<br>";
        private Keybind[] keybinds;

        public ConsoleDialogueView(Keybind[] keybinds)
        {
            this.keybinds = keybinds;
        }

        public void DisplayLine(LineData line)
        {
            string displayType = line.Tags.GetValueOrDefault("display", "newline");
            string block = line.Tags.GetValueOrDefault("block", "yes");
            string[] splitLines = line.Text.Split(LINE_BREAK);

            Console.ForegroundColor = ColorByName(line.Tags.GetValueOrDefault("color", "white"));

            foreach(string linePart in splitLines)
            {
                Console.Write(linePart);
                while(block == "yes") 
                {
                    if(!ConsoleDisplay.TryReadKey(true, this.keybinds, out ConsoleKeyInfo keyInfo))
                    { 
                        Console.ResetColor();
                        return;
                    }

                    if(keyInfo.Key == ConsoleKey.Enter)
                        break;
                }
            }

            Console.ResetColor();
            
            if(displayType == "newline")
                Console.WriteLine();
        }

        public int DisplayChoices(string[] choices)
        {
            return ConsoleDisplay.Menu(choices, keybinds);
        }

        private static ConsoleColor ColorByName(string name)
        {
            if(Enum.TryParse(name, true, out ConsoleColor color))
            {
                return color;
            } 
            else
            {
                return ConsoleColor.White;
            }
        }
    }
}
