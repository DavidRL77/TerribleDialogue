using System;
using System.Collections.Generic;

namespace TerribleDialogue
{
    /// <summary>
    /// Provides an easy way to manage multiple dialogue engines, exposing several useful callbacks.
    /// </summary>
    public class DialogueManager
    {
        public delegate void TagProcessor(string key, string value);

        public bool InDialogue => engine != null;
        public IReadOnlyDictionary<string, string> CurrentTags => engine.CurrentTags;

        private DialogueEngine engine;

        /// <summary>
        /// Called when a new engine is passed into the manager
        /// </summary>
        public event Action OnStart;

        /// <summary>
        /// Called when the engine lands on dialogue line
        /// </summary>
        public event Action<string> OnLine;

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

        private Dictionary<string, List<TagProcessor>> tagProcessors = new Dictionary<string, List<TagProcessor>>();

        public void BeginDialogue(DialogueEngine engine)
        {
            OnStart?.Invoke();

            this.engine = engine;
            Next();

        }
        /// <summary>
        /// Process dialogue until the next stop
        /// </summary>
        public void Next()
        {
            engine.Step();

            // When the engine has choices, it won't have a line, so check the choices first
            if(engine.PendingChoices.Length > 0)
            {
                OnChoices?.Invoke(engine.PendingChoices);
                return;
            }

            if(!engine.HasLine)
            {
                OnStop?.Invoke();
                return;
            }

            if(engine.IsDialogueOver)
            {
                EndDialogue();
                return;
            }

            foreach(KeyValuePair<string, string> kvp in engine.CurrentTags)
            {
                CallTagProcessors(kvp.Key, kvp.Value);
            }

            OnLine?.Invoke(engine.CurrentText);
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

        public void AddTagProcessor(string tagType, TagProcessor tagProcessor)
        {
            if(!tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors))
            {
                processors = new List<TagProcessor>();
                tagProcessors[tagType] = processors;
            }

            processors.Add(tagProcessor);
        }

        public void RemoveTagProcessor(string tagType, TagProcessor tagProcessor)
        {
            if(tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors))
            {
                if(processors.Remove(tagProcessor) && processors.Count == 0)
                {
                    tagProcessors.Remove(tagType);
                }
            }
        }

        private void CallTagProcessors(string tagType, string value)
        {
            if(tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors))
            {
                foreach(TagProcessor tagProcessor in processors)
                {
                    tagProcessor.Invoke(tagType, value);
                }
            }
        }
    }
}
