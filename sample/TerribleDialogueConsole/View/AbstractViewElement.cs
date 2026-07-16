using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View
{
    internal abstract class AbstractViewElement : IViewElement
    {
        public bool Visible { get; private set; }

        public void Show()
        {
            Visible = true;
            OnShow();
        }

        public void Hide()
        {
            Visible = false;
            OnHide();
        }

        protected abstract void OnShow();
        protected abstract void OnHide();

    }
}
