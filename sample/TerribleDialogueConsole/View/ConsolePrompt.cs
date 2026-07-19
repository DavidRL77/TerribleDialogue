using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerribleDialogueConsole.View.Input;

namespace TerribleDialogueConsole.View
{
    internal class ConsolePrompt : AbstractViewElement
    {
        public required IInputHandler<ConsoleKeyInfo> InputHandler { get; init; }
        public required Action<string> OnComplete { get; init; }
        public Func<ConsoleKeyInfo, bool> StopCondition { get; init; } = DefaultStopCondition;

        [SetsRequiredMembers]
        public ConsolePrompt(IInputHandler<ConsoleKeyInfo> inputHandler, Action<string> onComplete, Func<ConsoleKeyInfo, bool> stopCondition)
        {
            InputHandler = inputHandler;
            OnComplete = onComplete;
            StopCondition = stopCondition;
        }

        public ConsolePrompt() { }

        protected override void OnShow()
        {
            StringBuilder result = new StringBuilder();

            while(Visible)
            {
                if(!InputHandler.TryGetInput(out ConsoleKeyInfo input))
                    continue;

                if(StopCondition.Invoke(input))
                    break;

                result.Append(input.KeyChar);
            }

            if(Visible)
                OnComplete.Invoke(result.ToString());
        }

        protected override void OnHide()
        {
            // Doesn't actually need to hide
        }

        private static bool DefaultStopCondition(ConsoleKeyInfo keyInfo) => keyInfo.Key == ConsoleKey.Enter;
    }
}
