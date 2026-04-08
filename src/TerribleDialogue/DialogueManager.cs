using System;
using System.Collections.Generic;
using TerribleDialogue.Data;

namespace TerribleDialogue
{
    /// <summary>
    /// Provides an easy way to manage multiple dialogue engines, exposing several useful callbacks.
    /// </summary>
    public class DialogueManager
    {
        public delegate void TagProcessor(string key, string value);
        public delegate void CallHandler(CallData callData);

        public bool InDialogue => engine != null;

        private DialogueEngine engine;

        /// <summary>
        /// Called when a new engine is passed into the manager
        /// </summary>
        public event Action OnStart;

        /// <summary>
        /// Called when the engine lands on dialogue line
        /// </summary>
        public event Action<LineData> OnLine;

        /// <summary>
        /// Called when the engine lands on a set of choices
        /// </summary>
        public event Action<string[]> OnChoices;

        /// <summary>
        /// Called when the engine stops and doesn't have a line or choices. <br></br>
        /// Can be useful to control if dialogue should flow all the way until the end, or stop at certain points.
        /// </summary>
        public event Action OnStop;

        /// <summary>
        /// Called when the engine has no more dialogue left
        /// </summary>
        public event Action OnEnd;

        private MappedCallbacks<string, TagProcessor> tagProcessors = new MappedCallbacks<string, TagProcessor>();
        private MappedCallbacks<string, CallHandler> callHandlers = new MappedCallbacks<string, CallHandler>();

        public void BeginDialogue(DialogueEngine engine)
        {
            OnStart?.Invoke();

            this.engine = engine;
            
            // If the engine is already at a valid statement, avoid skipping over it
            if(engine.IsAtValidStatement())
                Next();
            else
                ProcessEngine();
        }
        /// <summary>
        /// Process dialogue until the next stop
        /// </summary>
        public void Next()
        {
            engine.Step();

            ProcessEngine();
        }

        private void ProcessEngine()
        {
            if(engine.HasCall)
            {
                CallData callData = engine.CurrentCall;
                callHandlers.Invoke(callData.Name, c => c.Invoke(callData));
                Next(); // Don't stop at calls since those should be handled at once
                return;
            }

            // When the engine has choices, it won't have a line, so check the choices first
            if(engine.PendingChoices.Length > 0)
            {
                OnChoices?.Invoke(engine.PendingChoices);
                return;
            }

            if(engine.HasLine)
            {
                foreach(KeyValuePair<string, string> kvp in engine.CurrentLine.Tags)
                {
                    tagProcessors.Invoke(kvp.Key, c => c.Invoke(kvp.Key, kvp.Value));
                }

                OnLine?.Invoke(engine.CurrentLine);
                return;
            }

            if(engine.IsDialogueOver)
            {
                EndDialogue();
                return;
            }

            // If engine doesn't have anything of the above, it means we've stopped somewhere.
            OnStop?.Invoke();
        }

        public void AddChoice(int choiceIndex) => engine.AddChoice(choiceIndex);

        /// <summary>
        /// Stops processing this specific engine
        /// </summary>
        public void EndDialogue()
        {
            engine = null;
            OnEnd?.Invoke();
        }

        public void AddTagProcessor(string tagType, TagProcessor tagProcessor) => tagProcessors.AddCallback(tagType, tagProcessor);

        public void RemoveTagProcessor(string tagType, TagProcessor tagProcessor) => tagProcessors.RemoveCallback(tagType, tagProcessor);

        public void AddCallHandler(string name, CallHandler callHandler) => callHandlers.AddCallback(name, callHandler);

        public void RemoveCallHandler(string name, CallHandler callHandler) => callHandlers.RemoveCallback(name, callHandler);
    }
}
