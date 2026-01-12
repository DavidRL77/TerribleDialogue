using System.Collections.Generic;

namespace Davicro.TerribleDialogue
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