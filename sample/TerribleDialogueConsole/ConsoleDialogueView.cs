using TerribleDialogue.Data;
using TerribleDialogueConsole.View;

namespace TerribleDialogue
{
    internal class ConsoleDialogueView : IDialogueView
    {
        public enum InputResult
        {
            Handled,
            Unhandled,
            Cancelled
        }

        private const string CHOICE_DISPLAY_CHARS = "1234567890abcdefghijklmnopqrstuvwxyz";
        private const string LINE_BREAK = "<br>";

        private readonly Func<ConsoleKeyInfo, InputResult> inputHandler;

        public ConsoleDialogueView() : this(DefaultInputHandler)
        {
        }

        /// <summary>
        /// Custom input handling, return true if the input has already been handled, false if the view should handle it instead.
        /// </summary>
        /// <param name="inputHandler"></param>
        public ConsoleDialogueView(Func<ConsoleKeyInfo, InputResult> inputHandler)
        {
            this.inputHandler = inputHandler;
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
                    if(!TryGetKey(out ConsoleKeyInfo keyInfo))
                        return;

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
        	Console.ForegroundColor = ConsoleColor.Gray;
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
                if(!TryGetKey(out ConsoleKeyInfo keyInfo))
                    return -1;

                char choice = keyInfo.KeyChar;
                choiceIndex = CHOICE_DISPLAY_CHARS.IndexOf(choice);
            }

            for(int i = displayableChoices.Count - 1; i >= 0 && Console.CursorTop > 0; i--)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r");
            }

            Console.WriteLine($"> {choices[choiceIndex]}");
            Console.ResetColor();
            return choiceIndex;
        }


        private bool TryGetKey(out ConsoleKeyInfo keyInfo)
        {
            // Read keys until the input is no longer handled outside
           while(true)
            {
                keyInfo = Console.ReadKey(true);
                switch(inputHandler.Invoke(keyInfo))
                {
                    case InputResult.Handled:
                        continue;
                    case InputResult.Unhandled:
                        return true;
                    case InputResult.Cancelled:
                        return false;
                }
            }
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

        // Always delegates the input to the view
        private static InputResult DefaultInputHandler(ConsoleKeyInfo keyInfo) => InputResult.Unhandled;
    }
}
