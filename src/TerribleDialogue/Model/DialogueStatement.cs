using System;
using System.Collections.Generic;
using System.Text;

namespace Davicro.TerribleDialogue.Model
{
    public abstract record DialogueStatement
    {
        /// <summary>
        /// Whether execution of statements should stop here until prompted to continue
        /// </summary>
        public abstract bool IsYielding { get; }

        public sealed record Line : DialogueStatement
        {
            // Lines are always yielding since the data they provide needs to be processed
            public override bool IsYielding => true;

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
            // A goto statement can be yielding or not, depending on whether the change of set/node should stop processing
            public override bool IsYielding => isYielding;

            public FlowAction Action { get; }
            private readonly bool isYielding;

            public Goto(FlowAction action, bool isYielding = true)
            {
                Action = action;
                this.isYielding = isYielding;
            }
        }
    }
}
