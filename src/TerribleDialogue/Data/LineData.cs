using System.Collections.Generic;

namespace TerribleDialogue.Data
{
    public class LineData
    {
        public string Text { get; }
        public IReadOnlyDictionary<string, string> Tags { get; }

        public LineData(string text, IReadOnlyDictionary<string, string> tags)
        {
            Text = text;
            Tags = tags;
        }
    }
}
