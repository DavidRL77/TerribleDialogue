using System;
using System.Collections.Generic;

namespace TerribleDialogue.Model
{
    public abstract record DialogueStatement
    {
        /// <summary>
        /// Whether execution of statements should stop here until prompted to continue
        /// </summary>
        public abstract bool IsYielding { get; }
        public DialogueStatement[][] Branches { get; protected set; } = Array.Empty<DialogueStatement[]>();

        /// <summary>
        /// Default statement with exactly one branch and the rest of statements within it.
        /// </summary>
        public sealed record Root : DialogueStatement
        {
            public override bool IsYielding => false;

            public Root(DialogueStatement[] statements)
            {
                this.Branches = new DialogueStatement[1][];
                this.Branches[0] = statements;
            }
        }

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

            public Goto(FlowAction action, bool isYielding)
            {
                Action = action;
                this.isYielding = isYielding;
            }
        }

        public sealed record Choice : DialogueStatement
        {
            public override bool IsYielding => true;
            public string[] Choices { get; }

            public Choice(string[] choices, DialogueStatement[][] branches)
            {
                Choices = choices;
                Branches = branches;
            }
        }

        public sealed record Call : DialogueStatement
        {
            public override bool IsYielding => true;
            public string Name { get; }
            public object[] Args { get; }

            public Call(string name, object[] args)
            {
                Name = name;
                Args = args;
            }
        }
    }
}
