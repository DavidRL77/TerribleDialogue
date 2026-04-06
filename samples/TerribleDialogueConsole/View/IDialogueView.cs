using TerribleDialogue.Data;

namespace TerribleDialogueConsole.View
{
    internal interface IDialogueView
    {
        public void DisplayLine(LineData line);
        public int DisplayChoices(string[] choices);
    }
}
