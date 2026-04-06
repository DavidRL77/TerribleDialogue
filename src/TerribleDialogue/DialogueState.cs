using TerribleDialogue.Data;
using System.Collections.Generic;
using System;

namespace TerribleDialogue
{
    public sealed class DialogueState
    {
        /// <summary>
        /// Has dialogue run out or been explicitly ended 
        /// </summary>
        public bool IsDialogueOver { get; set; }
        /// <summary>
        /// Information about the line we are currently at (null if none)
        /// </summary>
        public LineData CurrentLine { get; set; }
        public CallData CurrentCall { get; set; }
        public string[] PendingChoices { get; set; } = Array.Empty<string>();
        public string CurrentSet { get; set; }
        public string CurrentNode { get; set; }
        /// <summary>
        /// Path that points us down to the current statment
        /// </summary>
        public List<Pointer> StatementPath { get; set; } = new List<Pointer>();
        public string PreviousSet { get; set; }
        public string PreviousNode { get; set; }
        public HashSet<string> DiscardedNodes { get; set; } = new HashSet<string>();
        /// <summary>
        /// Whenever a choice is hit, use the next element in queue
        /// </summary>
        public Queue<int> ChoiceQueue { get; set; } = new Queue<int>();
    }
}
