using Davicro.TerribleDialogue;

namespace TerribleDialogueConsole
{
    internal record Character
    {
        public string Name { get; }
        public DialogueEngine Engine { get; }

        public bool DeleteWhenOver { get; }

        public Character(string name, DialogueEngine engine, bool deleteWhenOver = false)
        {
            Name = name;
            Engine = engine;
            DeleteWhenOver = deleteWhenOver;
        }
    }
}
