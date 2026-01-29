using Davicro.TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Davicro.TerribleDialogue
{
    public class DialogueProcessor
    {
        public enum StepResult
        {
            Line,
            ChangeSet,
            ChangeNode,
            End
        }

        public delegate int RandProvider(int inclusiveMin, int exclusiveMax);

        private const string START_SET = "default";

        public bool IsDialogueOver { get; private set; }
        public string CurrentText { get; private set; }
        public IReadOnlyDictionary<string,string> CurrentTags { get; private set; }
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

        private bool HasNextStatement()
        {
            return statementIndex < currentNode.Statements.Length;
        }

        /// <summary>
        /// Advances the dialogue to the next stopping point
        /// </summary>
        /// <returns>Where we stopped</returns>
        public StepResult Step()
        {
            if (!HasNextStatement())
            {
                EndDialogue();
                return StepResult.End;
            }

            do
            {
                DialogueStatement statement = GetNextStatement();

                StepResult result = StepResult.End;
                switch(statement)
                {
                    case DialogueStatement.Goto g:
                        result = ProcessFlowAction(g.Action);
                        break;
                    case DialogueStatement.Line l:
                        CurrentText = l.Text;
                        CurrentTags = l.Tags;
                        result = StepResult.Line;
                        break;
                }

                if(statement.IsBlocking)
                    return result;

            } while(HasNextStatement());

            return StepResult.End;
        }
        
        private StepResult ProcessFlowAction(FlowAction action)
        {
            switch(action)
            {
                case FlowAction.NodeAction n:
                    SetNode(n.Id);
                    return StepResult.ChangeNode;
                case FlowAction.SetAction s:
                    SetSet(s.Id);
                    return StepResult.ChangeSet;
                case FlowAction.RandomAction r:
                    SetRandomNode(r.Discard);
                    return StepResult.ChangeNode;
                case FlowAction.PreviousAction:
                    string node = previousNode;
                    SetSet(previousSet);
                    SetNode(node);
                    return StepResult.ChangeSet;
                case FlowAction.EndAction e:
                    EndDialogue();
                    return StepResult.End;
                default:
                    throw new NotImplementedException($"No flow action for '{action.GetType()}'");
            }
        }

        private DialogueStatement GetNextStatement()
        {
            if(!HasNextStatement())
                return null;

            DialogueStatement statement = currentNode.Statements[statementIndex++];
            return statement;
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
                IsDialogueOver = false; // Re-enables dialogue if explicitely setting a new set
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
            IsDialogueOver = true;
            CurrentText = null;
            CurrentTags = null;
        }
    }
}