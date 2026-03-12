using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerribleDialogue.Data;

namespace TerribleDialogueConsole.View
{
    internal interface IDialogueView
    {
        public void DisplayLine(LineData line);
        public int DisplayChoices(string[] choices);
    }
}
