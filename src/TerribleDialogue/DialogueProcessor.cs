using Davicro.TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Davicro.TerribleDialogue
{
    public class DialogueProcessor
    {
        public delegate int RandProvider(int inclusiveMin, int exclusiveMax);

        private const string START_SET = "default";

        public bool HasEndedDialogue { get; private set; }
        public string CurrentSetId => currentSet.Id;
        public string CurrentNodeId => currentNode.Id;

        private readonly DialogueObject dialogueObject;
        private DialogueSet currentSet;
        private DialogueNode currentNode;
        private int statementIndex = 0;
        private HashSet<string> discardedNodes = new HashSet<string>();

        private string previousNode;
        private string previousSet;

        // I'm using a delegate provider for random numbers since I don't want to couple it to C#'s System.Random,
        // so that the project is better integrated into wherever it might be used.
        // Unity for example has its own Random that uses its own seed.
        private readonly RandProvider randProvider;

        public DialogueProcessor(DialogueObject obj, RandProvider randProvider)
        {
            this.dialogueObject = obj;
            this.randProvider = randProvider;
            SetSet(START_SET);
        }

        public bool HasNextStatement()
        {
            return statementIndex < currentNode.Statements.Length;
        }

        public DialogueStatement.Line GetNextLine()
        {
            do
            {
                DialogueStatement statement = GetNextStatement();

                switch(statement)
                {
                    case DialogueStatement.Goto g:
                        // Early return so the manager can decide if to continue after a goto or not
                        ProcessFlowAction(g.Action);
                        return null;
                    case DialogueStatement.Line l:
                        return l;
                }
            }
            while(HasNextStatement());
            

            return null;

        }
        
        private void ProcessFlowAction(FlowAction action)
        {
            switch(action)
            {
                case FlowAction.NodeAction n:
                    SetNode(n.Id);
                    break;
                case FlowAction.SetAction s:
                    SetSet(s.Id);
                    break;
                case FlowAction.RandomAction r:
                    SetRandomNode(r.Discard);
                    break;
                case FlowAction.PreviousAction:
                    string node = previousNode;
                    SetSet(previousSet);
                    SetNode(node);
                    break;
                case FlowAction.EndAction e:
                    EndDialogue();
                    break;
            }
        }

        private DialogueStatement GetNextStatement()
        {
            if(!HasNextStatement())
                return null;

            return currentNode.Statements[statementIndex++];
        }

        public void SetNode(string id)
        {
            if(currentSet.Nodes.TryGetValue(id, out DialogueNode node))
            {
                currentNode = node;
                this.statementIndex = 0;
            }
            else
            {
                throw new Exception($"No node with id '{id}'");
            }
        }

        public void SetSet(string id)
        {
            if(dialogueObject.Sets.TryGetValue(id, out DialogueSet set))
            {
                if(currentSet != null)
                {
                    previousSet = currentSet.Id;
                    previousNode = currentNode.Id;
                }

                currentSet = set;
                HasEndedDialogue = false; // Re-enables dialogue if explicitely setting a new set
                discardedNodes.Clear(); // Doesn't make sense to keep nodes discarded when changing sets, even if back to the same one
                ProcessFlowAction(set.StartFlowAction);
            }
            else
            {
                throw new Exception($"No set with id '{id}'");
            }

        }

        public bool HasSet(string id)
        {
            return dialogueObject.Sets.ContainsKey(id);
        }

        public void SetRandomNode(bool discardCurrent)
        {
            if(currentNode != null && discardCurrent)
                discardedNodes.Add(currentNode.Id);

            // All nodes except the discarded ones
            string[] availableNodes = discardedNodes.Count == 0 ? currentSet.Nodes.Keys.ToArray() : currentSet.Nodes.Select(kvp => kvp.Key).Where(id => !discardedNodes.Contains(id)).ToArray();
            if(availableNodes.Length == 0)
            {
                EndDialogue();
                return;
            }

            SetNode(availableNodes[randProvider.Invoke(0, availableNodes.Length)]);
        }

        public void EndDialogue()
        {
            HasEndedDialogue = true;
        }
    }
}