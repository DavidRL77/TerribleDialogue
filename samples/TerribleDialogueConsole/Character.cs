using Davicro.TerribleDialogue;

namespace TerribleDialogueConsole
{
    internal record Character
    {
        public string Name { get; }
        public DialogueEngine Engine { get; }

        public Character(string name, DialogueEngine engine) {
            Name = name;
            Engine = engine;
        }
    }
}
