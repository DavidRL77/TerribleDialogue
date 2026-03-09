using System.Collections.Generic;

namespace TerribleDialogue.Model
{
    public record DialogueObject
    {
        public Dictionary<string, DialogueSet> Sets { get; }

        public DialogueObject(Dictionary<string, DialogueSet> sets)
        {
            Sets = sets;
        }
    }
}