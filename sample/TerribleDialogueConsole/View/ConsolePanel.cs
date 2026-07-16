using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View
{
    internal class ConsolePanel : AbstractViewElement
    {
        private List<IViewElement> Elements { get; }

        public ConsolePanel(params IViewElement[] elements)
        {
            Elements = new List<IViewElement>(elements);
        }

        protected override void OnShow()
        {
            foreach(IViewElement element in Elements)
            {
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
