using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View
{
    internal class ConsolePanel : AbstractViewElement
    {
        public int Count => Elements.Count;
        private List<IViewElement> Elements { get; }

        public ConsolePanel(params IViewElement[] elements)
        {
            Elements = new List<IViewElement>(elements);
        }

        protected override void OnShow()
        {
            foreach(IViewElement element in Elements)
            {
                if(!Visible) // Other elements might hide the panel before it's done rendering everything
                    break;

                element.Show();
            }
        }

        protected override void OnHide()
        {
            foreach(IViewElement element in Elements)
            {
                element.Hide();
            }

            Console.Clear(); // TEMP
        }

        public void AddElement(IViewElement element)
        {
            Elements.Add(element);

            if(Visible)
                element.Show();
        }

        public void RemoveElement(IViewElement element)
        {
            Elements.Remove(element);
            element.Hide();
        }

        public void ClearElements()
        {
            Hide();
            Elements.Clear();
            Show();
        }

    }
}
