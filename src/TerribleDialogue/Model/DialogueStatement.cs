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
        public abstract bool IsBlocking { get; }

        public sealed record Line : DialogueStatement
        {
            public override bool IsBlocking => true;

            public string Text { get; }
            public Dictionary<string, string> Tags { get; }

            public Line(string text, Dictionary<string, string> tags, bool isBlocking = true)
            {
                Text = text;
                Tags = tags;
            }
        }

        public sealed record Goto : DialogueStatement
        {
            public override bool IsBlocking => isBlocking;

            public FlowAction Action { get; }
            private readonly bool isBlocking;

            public Goto(FlowAction action, bool isBlocking = true)
            {
                Action = action;
                this.isBlocking = isBlocking;
            }
        }
    }
}
