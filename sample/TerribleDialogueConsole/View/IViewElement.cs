using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View
{
    internal interface IViewElement
    {
        public bool Visible { get; }

        public void Show();
        public void Hide();
    }
}
