using System;
using System.Collections.Generic;
using System.Text;

namespace Davicro.TerribleDialogue.Model
{
    public abstract record DialogueStatement
    {
        public sealed record Line : DialogueStatement
        {
            public string Text { get; }
            public Dictionary<string, string> Tags { get; }

            public Line(string text, Dictionary<string, string> tags)
            {
                Text = text;
                Tags = tags;
            }
        }

        public sealed record Goto : DialogueStatement
        {
            public FlowAction Action { get; }

            public Goto(FlowAction action)
            {
                Action = action;
            }
        }
    }
}
