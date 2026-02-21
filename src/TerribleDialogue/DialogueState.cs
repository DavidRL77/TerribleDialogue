using Davicro.TerribleDialogue.Model;
using System.Collections.Generic;

namespace TerribleDialogue
{
    internal sealed class DialogueState
    {
        public static readonly DialogueState END_STATE = new DialogueState() { IsDialogueOver = true };

        /// <summary>
        /// Has dialogue run out or been explicitly ended 
        /// </summary>
        public bool IsDialogueOver { get; internal set; }
        /// <summary>
        /// Has a line available to read
        /// </summary>
        public bool HasLine { get; internal set; }
        /// <summary>
        /// The text of the statement we're currently in
        /// </summary>
        public string CurrentText { get; internal set; }
        /// <summary>
        /// The tags of the statement we're currently in
        /// </summary>
        public IReadOnlyDictionary<string, string> CurrentTags { get; internal set; }
        public DialogueSet CurrentSet { get; internal set; }
        public DialogueNode CurrentNode { get; internal set; }
        /// <summary>
        /// Path that points us down to the current statment
        /// </summary>
        public List<Pointer> StatementPath { get; internal set; } = new List<Pointer>();
        public string PreviousSet { get; internal set; }
        public string PreviousNode { get; internal set; }
        public HashSet<string> DiscardedNodes { get; internal set; } = new HashSet<string>();
        public string[] PendingChoices { get; internal set; } = new string[0];
        /// <summary>
        /// Whenever a choice is hit, use the first element in queue
        /// </summary>
        public Queue<int> ChoiceQueue { get; internal set; } = new Queue<int>();
    }
}
