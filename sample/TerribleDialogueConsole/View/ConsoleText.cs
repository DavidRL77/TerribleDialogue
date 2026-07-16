using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View
{
    internal class ConsoleText : AbstractViewElement
    {
        public string Text { get; }
        public ConsoleColor ForegroundColor { get; }
        public ConsoleColor BackgroundColor { get; }

        public bool Newline { get; }

        public ConsoleText(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor, bool newline = true)
        {
            Text = text;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
            Newline = newline;
        }

        protected override void OnShow()
        {
            Console.ForegroundColor = ForegroundColor;
            Console.BackgroundColor = BackgroundColor;

            if(Newline)
                Console.WriteLine(Text);
            else
                Console.Write(Text);

            Console.ResetColor();
        }

        protected override void OnHide()
        {
            // ??
        }

    }
}
