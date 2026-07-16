using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerribleDialogueConsole.View.Input;

namespace TerribleDialogueConsole.View
{
    internal class ConsoleMenu<T> : AbstractViewElement
    {
        public const string DISPLAY_CHARS = "1234567890abcdefghijklmnopqrstuvwxyz";

        public required T[] Options { get; init; }
        public required IInputHandler<ConsoleKeyInfo> InputHandler { get; init; }
        public required Action<int, T> SelectionCallback { get; init; }
        public Func<T, string> OptionDisplayFunc { get; init; } = DefaultStringConversion;
        public ConsoleColor ForegroundColor { get; init; } = ConsoleColor.White;
        public ConsoleColor BackgroundColor { get; init; } = ConsoleColor.Black;


        [SetsRequiredMembers]
        public ConsoleMenu(T[] options, IInputHandler<ConsoleKeyInfo> inputHandler, Action<int, T> selectionCallback, 
            Func<T, string> optionDisplayFunc, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            // Only store the options we can actually display
            Options = options.Take(DISPLAY_CHARS.Length).ToArray();
            InputHandler = inputHandler;
            SelectionCallback = selectionCallback;
            this.OptionDisplayFunc = optionDisplayFunc;

            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;

        }

        public ConsoleMenu() { }

        protected override void OnShow()
        {
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;

            for(int i = 0; i < Options.Length; i++)
            {
                string option = OptionDisplayFunc.Invoke(Options[i]);
                Console.WriteLine($"{DISPLAY_CHARS[i]}. {option}");
            }

            Console.ResetColor();

            while(Visible)
            {
                if(!InputHandler.TryGetInput(out ConsoleKeyInfo keyInfo))
                    continue;

                int selected = DISPLAY_CHARS.IndexOf(keyInfo.KeyChar);
                if(selected >= 0 && selected <= Options.Length)
                {
                    SelectionCallback?.Invoke(selected, Options[selected]);
                    break;
                }
            }

        }

        protected override void OnHide()
        {
            // ??
        }

        private static string DefaultStringConversion(T obj) => obj.ToString();
    }
}
