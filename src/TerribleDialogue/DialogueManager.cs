using System;
using System.Collections.Generic;

namespace Davicro.TerribleDialogue
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
        public event Action OnStart;
        public event Action<string> OnLine;
        public event Action<string[]> OnChoices;
        public event Action OnEnd;

        private Dictionary<string, List<TagProcessor>> tagProcessors = new Dictionary<string, List<TagProcessor>>();

        public void BeginDialogue(DialogueEngine engine) {
            OnStart.Invoke();

            this.engine = engine;
            Next();

        }
        /// <summary>
        /// Process dialogue until the next stop
        /// </summary>
        public void Next() {
            engine.Step();

            if(engine.PendingChoices.Length > 0) {
                OnChoices?.Invoke(engine.PendingChoices);
                return;
            }


            if(engine.IsDialogueOver || !engine.HasLine) {
                EndDialogue();
                return;
            }
            foreach(KeyValuePair<string, string> kvp in engine.CurrentTags) {
                CallTagProcessors(kvp.Key, kvp.Value);
            }

            OnLine?.Invoke(engine.CurrentText);
        }

        public void AddChoice(int choiceIndex) => engine.AddChoice(choiceIndex);

        public void EndDialogue() {
            engine = null;
            OnEnd?.Invoke();
        }

        public void AddTagProcessor(string tagType, TagProcessor tagProcessor) {
            if(!tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors)) {
                processors = new List<TagProcessor>();
                tagProcessors[tagType] = processors;
            }

            processors.Add(tagProcessor);
        }

        public void RemoveTagProcessor(string tagType, TagProcessor tagProcessor) {
            if(tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors)) {
                if(processors.Remove(tagProcessor) && processors.Count == 0) {
                    tagProcessors.Remove(tagType);
                }
            }
        }

        private void CallTagProcessors(string tagType, string value) {
            if(tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors)) {
                foreach(TagProcessor tagProcessor in processors) {
                    tagProcessor.Invoke(tagType, value);
                }
            }
        }
    }
}
