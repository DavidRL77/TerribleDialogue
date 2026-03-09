using TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using TerribleDialogue;
using TerribleDialogue.Data;

namespace TerribleDialogue
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

        // Pointer at the root of a dialogue tree, branch 0, statement 0
        private static Pointer RootPointer = Pointer.BranchStart(0);

        public DialogueObject DialogueObject => dialogueObject;
        public bool IsDialogueOver => state.IsDialogueOver;
        public bool HasLine => state.CurrentLine != null;
        public LineData? CurrentLine => state.CurrentLine;
        public string CurrentSetId => state.CurrentSet;
        public string CurrentNodeId => state.CurrentNode;
        public string[] PendingChoices => state.PendingChoices;


        private DialogueSet CurrentSet => dialogueObject.Sets[CurrentSetId];
        private DialogueNode CurrentNode => CurrentSet.Nodes[CurrentNodeId];

        private readonly DialogueObject dialogueObject;
        private DialogueState state = new DialogueState();
        private readonly RandProvider randProvider;

        public DialogueEngine(DialogueObject obj, RandProvider randProvider, string startSet = "default")
        {
            this.dialogueObject = obj;
            this.randProvider = randProvider;
            SetSet(startSet);
        }

        private bool HasNextStatement()
        {
            return state.StatementPath.Count > 0;
        }

        /// <summary>
        /// Advances the dialogue to the next stopping point.
        /// When stopping check <see cref="HasLine"/> to see if there's a line available to read.
        /// </summary>
        public void Step()
        {
            if(!HasNextStatement())
            {
                EndDialogue();
                return;
            }


            if(PendingChoices.Length > 0 && !TryResolveChoice())
                return;

            do
            {
                // Reset values
                state.CurrentLine = null;
                state.PendingChoices = Array.Empty<string>();

                DialogueStatement statement = Advance();
                if(statement == null)
                {
                    EndDialogue();
                    return;
                }

                bool yield = statement.IsYielding;
                switch(statement)
                {
                    case DialogueStatement.Goto g:
                        ProcessFlowAction(g.Action);
                        break;
                    case DialogueStatement.Line l:
                        state.CurrentLine = new LineData(l.Text, l.Tags);
                        break;
                    case DialogueStatement.Choice c:
                        if(!TryResolveChoice())
                        {
                            state.PendingChoices = c.Choices;
                        } 
                        else
                        {
                            // Choices that are resolved automatically shouldn't yield
                            yield = false;
                        }
                        break;
                }

                // A yielding statement returns control until prompted to step again
                if(yield)
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

        private bool TryResolveChoice()
        {
            if(!state.ChoiceQueue.TryDequeue(out int choiceIndex))
                return false;

            state.StatementPath.Add(Pointer.BranchStart(choiceIndex));
            return true;
        }

        /// <summary>
        /// Advances the path to the next statement and resolves any branch backtracking. Skips over unresolved branches.
        /// </summary>
        /// <returns>The statement after advancing</returns>
        /// <exception cref="Exception"></exception>
        private DialogueStatement Advance()
        {
            if(state.StatementPath.Count == 0)
                throw new Exception("No pointer path");

            // Build a stack of branches as we traverse the tree so that we can then resolve upwards when we exit a branch
            Stack<DialogueStatement> branchStack = new Stack<DialogueStatement>();

            int depth = state.StatementPath.Count - 1;
            DialogueStatement current = CurrentNode.Root;

            // Build our stack of branch sizes up until our current depth
            for(int i = 0; i <= depth; i++)
            {
                // Only push the parent statements (the ones that have branches, not the leafs)
                branchStack.Push(current);

                Pointer pointer = state.StatementPath[i];
                DialogueStatement[] branch = current.Branches[pointer.Branch];
                if(branch.Length == 0 || pointer.StatementIndex < 0) // Nowhere else to go
                    break;

                current = branch[pointer.StatementIndex];
            }

            // Undo our steps propagating any out of bounds pointers
            while(branchStack.Count > 0)
            {
                // Try to advance the last pointer in the path, every time a pointer goes out of bounds,
                // the next pointer will get advanced, and so forth
                Pointer pointer = state.StatementPath[depth].Next();

                DialogueStatement parent = branchStack.Pop();
                if(pointer.StatementIndex >= parent.Branches[pointer.Branch].Length)
                {
                    // If the pointer is out of bounds we remove it and advance the next one
                    state.StatementPath.RemoveAt(depth);
                    current = null;
                } else
                {
                    // If the pointer is still in bounds, we leave the rest as-is
                    state.StatementPath[depth] = pointer;
                    current = parent.Branches[pointer.Branch][pointer.StatementIndex];
                    break;
                }

                depth--;
            }


            return current;
        }

        /// <summary>
        /// Add a choice to the queue to be processed when a choice statement is reached
        /// </summary>
        /// <param name="choiceIndex"></param>
        public void AddChoice(int choiceIndex)
        {
            state.ChoiceQueue.Enqueue(choiceIndex);
        }

        public void SetNode(string id)
        {
            if(CurrentSet.Nodes.ContainsKey(id))
            {
                state.CurrentNode = id;
                state.StatementPath.Clear();
                state.StatementPath.Add(RootPointer);
            } else
            {
                throw new Exception($"No node with id '{id}'");
            }
        }

        public void SetSet(string id)
        {
            if(dialogueObject.Sets.ContainsKey(id))
            {
                if(CurrentSetId != null)
                {
                    state.PreviousSet = CurrentSetId;
                    state.PreviousNode = CurrentNodeId;
                }

                state.CurrentSet = id;
                state.IsDialogueOver = false; // Re-enables dialogue if explicitely setting a new set
                state.DiscardedNodes.Clear(); // Doesn't make sense to keep nodes discarded when changing sets, even if back to the same one
                ProcessFlowAction(CurrentSet.StartFlowAction);
            } else
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
            string[] availableNodes = state.DiscardedNodes.Count == 0 ? CurrentSet.Nodes.Keys.ToArray() : CurrentSet.Nodes.Select(kvp => kvp.Key).Where(id => !state.DiscardedNodes.Contains(id)).ToArray();
            if(availableNodes.Length == 0)
            {
                EndDialogue();
                return;
            }

            SetNode(availableNodes[randProvider.Invoke(0, availableNodes.Length)]);
        }

        public void EndDialogue()
        {
            this.state = DialogueState.END_STATE;
        }
    }
}