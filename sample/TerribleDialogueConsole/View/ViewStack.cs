using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerribleDialogueConsole.View
{
    // Stack of views where only the one on top will be visible
    internal class ViewStack
    {
        public IViewElement CurrentView => views.TryPeek(out IViewElement view) ? view : null;

        private readonly Stack<IViewElement> views;

        public ViewStack(params IViewElement[] views)
        {
            this.views = new Stack<IViewElement>(views);

            if(this.views.TryPeek(out IViewElement element))
                element.Show();
        }

        public void Push(IViewElement element)
        {
            if(views.TryPeek(out IViewElement prevElement))
            {
                prevElement.Hide();
            }

            views.Push(element);
            element.Show();
        }

        public void Pop()
        {
            if(views.TryPop(out IViewElement popped))
            {
                popped.Hide();
            }

            if(views.TryPeek(out IViewElement element))
            {
                element.Show();
            }
        }

        public void Clear()
        {
            // Hide the visible view before clearing
            if(views.TryPeek(out IViewElement element))
            {
                element.Hide();
            }
            views.Clear();
        }
    }
}
