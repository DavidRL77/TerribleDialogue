using TerribleDialogue.Data;
using TerribleDialogueConsole;

namespace TerribleDialogueConsole.View
{
    internal class ConsoleDialogueView : IDialogueView
    {
        private const string LINE_BREAK = "<br>";
        private Keybind[] keybinds;
        private bool inputCancelled;

        public ConsoleDialogueView(Keybind[] keybinds)
        {
            this.keybinds = keybinds;
        }

        public bool DisplayLine(LineData line)
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
                    if(inputCancelled)
                    {
                        inputCancelled = false;
                        Console.ResetColor();
                        return false;
                    }

                    if(ConsoleDisplay.TryReadKey(true, keybinds, out ConsoleKeyInfo keyInfo) && keyInfo.Key == ConsoleKey.Enter)
                        break;
                }
            }

            Console.ResetColor();
            
            if(displayType == "newline")
                Console.WriteLine();

            return true;
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

        public void CancelInput()
        {
            inputCancelled = true;
        }
    }
}
