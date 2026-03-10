using TerribleDialogue.Data;
using System.Collections.Generic;
using System;

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
        /// Information about the line we are currently at (null if none)
        /// </summary>
        public LineData? CurrentLine { get; internal set; }
        public CallData? CurrentCall { get; internal set; }
        public string[] PendingChoices { get; internal set; } = Array.Empty<string>();
        public string CurrentSet { get; internal set; }
        public string CurrentNode { get; internal set; }
        /// <summary>
        /// Path that points us down to the current statment
        /// </summary>
        public List<Pointer> StatementPath { get; internal set; } = new List<Pointer>();
        public string PreviousSet { get; internal set; }
        public string PreviousNode { get; internal set; }
        public HashSet<string> DiscardedNodes { get; internal set; } = new HashSet<string>();
        /// <summary>
        /// Whenever a choice is hit, use the next element in queue
        /// </summary>
        public Queue<int> ChoiceQueue { get; internal set; } = new Queue<int>();
    }
}
