using Davicro.TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TerribleDialogue;
namespace Davicro.TerribleDialogue
{
    public class DialogueProcessor
    {
        public delegate int RandProvider(int inclusiveMin, int exclusiveMax);
        private const string START_SET = "default";


        public bool IsDialogueOver => state.IsDialogueOver;
        public bool HasLine => state.HasLine;
        public string CurrentText => state.CurrentText;
        public IReadOnlyDictionary<string, string> CurrentTags => state.CurrentTags;
        public string CurrentSetId => state.CurrentSet?.Id;
        public string CurrentNodeId => state.CurrentNode?.Id;



        private readonly DialogueObject dialogueObject;
        private readonly DialogueState state = new DialogueState();


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
            return state.CurrentStatement < state.CurrentNode.Statements.Length;
        }

        /// <summary>
        /// Advances the dialogue to the next stopping point.
        /// When stopping check <see cref="HasLine"/> to see if there's a line available to read.
        /// </summary>
        public void Step()
        {
            if (!HasNextStatement())
            {
                EndDialogue();
                return;
            }

            do
            {
                // Reset values
                state.CurrentText = null;
                state.CurrentTags = null;
                state.HasLine = false;

                DialogueStatement statement = GetNextStatement();
                switch(statement)
                {
                    case DialogueStatement.Goto g:
                        ProcessFlowAction(g.Action);
                        break;
                    case DialogueStatement.Line l:
                        state.CurrentText = l.Text;
                        state.CurrentTags = l.Tags;
                        state.HasLine = true;
                        break;
                }

                // A yielding statement returns control until prompted to step again
                if(statement.IsYielding)
                    return;

            } while(HasNextStatement());
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
                    string node = state.PreviousNode;
                    SetSet(state.PreviousSet);
                    SetNode(node);
                    break;
                case FlowAction.EndAction e:
                    EndDialogue();
                    break;
                default:
                    throw new NotImplementedException($"No flow action for '{action.GetType()}'");
            }
        }

        private DialogueStatement GetNextStatement()
        {
            if(!HasNextStatement())
                return null;

            DialogueStatement statement = state.CurrentNode.Statements[state.CurrentStatement++];
            return statement;
        }

        public void SetNode(string id)
        {
            if(state.CurrentSet.Nodes.TryGetValue(id, out DialogueNode node))
            {
                state.CurrentNode = node;
                state.CurrentStatement = 0;
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
                if(CurrentSetId != null)
                {
                    state.PreviousSet = CurrentSetId;
                    state.PreviousNode = CurrentNodeId;
                }

                state.CurrentSet = set;
                state.IsDialogueOver = false; // Re-enables dialogue if explicitely setting a new set
                state.DiscardedNodes.Clear(); // Doesn't make sense to keep nodes discarded when changing sets, even if back to the same one
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
            if(state.CurrentNode != null && discardCurrent)
                state.DiscardedNodes.Add(CurrentNodeId);

            // All nodes except the discarded ones
            string[] availableNodes = state.DiscardedNodes.Count == 0 ? state.CurrentSet.Nodes.Keys.ToArray() : state.CurrentSet.Nodes.Select(kvp => kvp.Key).Where(id => !state.DiscardedNodes.Contains(id)).ToArray();
            if(availableNodes.Length == 0)
            {
                EndDialogue();
                return;
            }

            SetNode(availableNodes[randProvider.Invoke(0, availableNodes.Length)]);
        }

        public void EndDialogue()
        {
            state.IsDialogueOver = true;
        }
    }
}