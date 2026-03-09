using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue.Data
{
    public readonly struct LineData
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
