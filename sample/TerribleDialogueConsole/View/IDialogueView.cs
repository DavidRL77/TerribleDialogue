using TerribleDialogue.Data;

namespace TerribleDialogueConsole.View
{
    internal interface IDialogueView
    {
        public bool DisplayLine(LineData line);
        public int DisplayChoices(string[] choices);
        public void CancelInput();
    }
}
