using Davicro.TerribleDialogue;

namespace TerribleDialogueConsole
{
    internal record Character
    {
        public string Name { get; }
        public DialogueProcessor Processor { get; }

        public Character(string name, DialogueProcessor processor)
        {
            Name = name;
            Processor = processor;
        }
    }
}
