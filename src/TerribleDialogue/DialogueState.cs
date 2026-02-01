using Davicro.TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue
{
    internal sealed class DialogueState
    {
        public bool IsDialogueOver { get; internal set; }
        public bool HasLine { get; internal set; }
        public string CurrentText { get; internal set; }
        public IReadOnlyDictionary<string, string> CurrentTags { get; internal set; }
        public DialogueSet CurrentSet { get; internal set; }
        public DialogueNode CurrentNode { get; internal set; }
        public int CurrentStatement { get; internal set; }
        public string PreviousSet { get; internal set; }
        public string PreviousNode { get; internal set; }
        public HashSet<string> DiscardedNodes { get; internal set; } = new HashSet<string>();
    }
}
