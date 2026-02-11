using Davicro.TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TerribleDialogue;
namespace Davicro.TerribleDialogue
{

    /// <summary>
    /// Main engine for processing dialogue flow, keeping track of its state.
    /// </summary>
    public class DialogueEngine
    {
        // I'm using a delegate provider for random numbers since I don't want to couple it to C#'s System.Random,
        // so that the project is better integrated into wherever it might be used.
        // Unity for example has its own Random that uses its own seed.
        public delegate int RandProvider(int inclusiveMin, int exclusiveMax);
        
        // Pointer at the root of a dialogue tree, without a branch
        private static Pointer RootPointer = new Pointer(0,-1);

        public DialogueObject DialogueObject => dialogueObject;
        public bool IsDialogueOver => state.IsDialogueOver;
        public bool HasLine => state.HasLine;
        public string CurrentText => state.CurrentText;
        public IReadOnlyDictionary<string, string> CurrentTags => state.CurrentTags;
        public string CurrentSetId => state.CurrentSet?.Id;
        public string CurrentNodeId => state.CurrentNode?.Id;
        

        private readonly DialogueObject dialogueObject;
        private readonly DialogueState state = new DialogueState();
        private readonly RandProvider randProvider;

        public DialogueEngine(DialogueObject obj, RandProvider randProvider, string startSet = "default")
        {
            this.dialogueObject = obj;
            this.randProvider = randProvider;
            SetSet(startSet);
        }

        private bool HasNextStatement()
        {
            return state.CurrentStatement < state.CurrentNode.Statements.Length;
        }

        private bool HasUnresolvedBranches()
        {
            return ResolveCurrentStatement().Branches.Length > 0;
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

            if(HasUnresolvedBranches())
            {
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

        private void Advance()
        {
            if(state.StatementPath.Count == 0)
                throw new Exception("No pointer path");

            // Where we keep the statements we've passed through
            // TODO: Only store the size of each container, no need for the whole statement
            // ALSO: This has the problem of the root node not actually having branches.
            Stack<DialogueStatement> stack = new Stack<DialogueStatement>();

            DialogueStatement current = null;
            // Build our stack of nested statements until depth-1
            // Since we don't really care about the leaf, we care about the branch
            for(int i = 0; i < state.StatementPath.Count-1; i++)
            {
                Pointer pointer = state.StatementPath[i];
                
                if(current == null)
                    current = state.CurrentNode.Statements[pointer.StatementIndex];
                else
                    current = current.Branches[pointer.Branch][pointer.StatementIndex];
                
                stack.Push(current);
            }
            
            // Undo our steps propagating any out of bounds pointers
            int depth = state.StatementPath.Count-1;
            while(stack.Count > 0)
            {
                Pointer pointer = state.StatementPath[depth];

                // Try to advance the last pointer in the path, every time a pointer goes out of bounds,
                // the next pointer will get advanced, and so forth
                pointer.Next();

                DialogueStatement parentStatement = stack.Pop();

                if(pointer.StatementIndex >= parentStatement.Branches[pointer.Branch].Length)
                {
                    // If the pointer is out of bounds we remove it and advance the next one
                    state.StatementPath.RemoveAt(depth);
                }
                else
                {
                    // If the pointer is still in bounds, we leave everything else as-is
                    break;
                }

                depth--;
            }
        }

        private DialogueStatement ResolveCurrentStatement()
        {
            return ResolveStatement(state.StatementPath.Count-1);
        }

        private DialogueStatement ResolveStatement(int depth)
        {
            // Root Pointer is the top-level statement
            Pointer rootPointer = state.StatementPath[0];
            DialogueStatement current = state.CurrentNode.Statements[rootPointer.StatementIndex];

            // Walk down the path of pointers until the end
            // We assume every pointer is valid
            for(int i = 1; i <= depth; i++)
            {
                Pointer pointer = state.StatementPath[i];
                current = current.Branches[pointer.Branch][pointer.StatementIndex];
            }


            return current;
        }

        public void SetNode(string id)
        {
            if(state.CurrentSet.Nodes.TryGetValue(id, out DialogueNode node))
            {
                state.CurrentNode = node;
                state.StatementPath.Clear();
                state.StatementPath.Add(RootPointer);
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