using System.Collections.Generic;

namespace Davicro.TerribleDialogue
{
    public record DialogueLine
    {
        public string Text { get; }
        public Dictionary<string, string> Tags { get; }

        public DialogueLine(string text, Dictionary<string, string> tags)
        {
            Text = text;
            Tags = tags;
        }
    }
}