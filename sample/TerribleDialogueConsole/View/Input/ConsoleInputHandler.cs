using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View.Input
{
    internal class ConsoleInputHandler : IInputHandler<ConsoleKeyInfo>
    {
        public bool Intercept { get; }

        public ConsoleInputHandler(bool intercept)
        {
            Intercept = intercept;
        }

        public bool TryGetInput(out ConsoleKeyInfo input)
        {
            input = Console.ReadKey(Intercept);
            return true;
        }
    }
}
