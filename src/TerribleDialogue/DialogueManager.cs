using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Davicro.TerribleDialogue
{
    public class DialogueManager
    {
        public delegate void TagProcessor(string key, string value);

        public bool InDialogue => processor != null;
        public IReadOnlyDictionary<string, string> CurrentTags => processor.CurrentTags;

        private DialogueProcessor processor;
        public event Action OnStart;
        public event Action<string> OnLine;
        public event Action OnEnd;

        private Dictionary<string, List<TagProcessor>> tagProcessors = new Dictionary<string, List<TagProcessor>>();

        public void BeginDialogue(DialogueProcessor processor)
        {
            OnStart.Invoke();

            this.processor = processor;
            Next();

        }

        public void Next()
        {
            DialogueProcessor.ProcessResult result = processor.Next();
            switch(result)
            {
                case DialogueProcessor.ProcessResult.ChangeSet:
                case DialogueProcessor.ProcessResult.ChangeNode:
                case DialogueProcessor.ProcessResult.End:
                    EndDialogue();
                    return;
            }


            foreach (KeyValuePair<string, string> kvp in processor.CurrentTags)
            {
                CallTagProcessors(kvp.Key, kvp.Value);
            }

            OnLine?.Invoke(processor.CurrentText);
        }

        public void EndDialogue()
        {
            processor = null;
            OnEnd?.Invoke();
        }

        public void AddTagProcessor(string tagType, TagProcessor tagProcessor)
        {
            if (!tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors))
            {
                processors = new List<TagProcessor>();
                tagProcessors[tagType] = processors;
            }

            processors.Add(tagProcessor);
        }

        public void RemoveTagProcessor(string tagType, TagProcessor tagProcessor)
        {
            if (tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors))
            {
                if (processors.Remove(tagProcessor) && processors.Count == 0)
                {
                    tagProcessors.Remove(tagType);
                }
            }
        }

        private void CallTagProcessors(string tagType, string value)
        {
            if (tagProcessors.TryGetValue(tagType, out List<TagProcessor> processors))
            {
                foreach (TagProcessor tagProcessor in processors)
                {
                    tagProcessor.Invoke(tagType, value);
                }
            }
        }
    }
}
