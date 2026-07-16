using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View.Input
{
    internal interface IInputHandler<T>
    {
        public bool TryGetInput(out T input);
    }
}
