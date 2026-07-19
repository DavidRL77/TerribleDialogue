using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerribleDialogueConsole.View.Input;

namespace TerribleDialogueConsole.View.Custom
{
    /// <summary>
    /// A panel consisting of two elements: list of text, and a prompt that when shown, blocks until the user presses enter,
    /// then removes itself from the panel. 
    /// </summary>
    internal class DialoguePanel : AbstractViewElement
    {        
        private readonly ConsolePanel textPanel;
        private readonly ConsolePrompt prompt;

        public DialoguePanel(IInputHandler<ConsoleKeyInfo> inputHandler, Action onPromptComplete)
        {
            textPanel = new ConsolePanel();
            prompt = new ConsolePrompt(inputHandler, s =>
            {
                onPromptComplete.Invoke();
            }, keyInfo => keyInfo.Key == ConsoleKey.Enter);

        }

        protected override void OnHide()
        {
            textPanel.Hide();
            prompt.Hide();
        }

        protected override void OnShow()
        {
            textPanel.Show();

            if(textPanel.Count > 0)
                prompt.Show();
        }

        public void ShowPrompt()
        {
            prompt.Show();
        }

        public void AddText(IViewElement element)
        {
            textPanel.AddElement(element);
        }


        public void Clear()
        {
            textPanel.ClearElements();
        }
    }
}
