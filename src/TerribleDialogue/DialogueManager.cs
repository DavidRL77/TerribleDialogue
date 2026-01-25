using Davicro.TerribleDialogue.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Davicro.TerribleDialogue
{
    public class DialogueManager
    {
        public delegate void TagProcessor(string key, string value);

        public bool InDialogue => currentProcessor != null;
        public DialogueStatement.Line CurrentLine => currentLine;

        private DialogueProcessor currentProcessor;
        private DialogueStatement.Line currentLine;

        public event Action OnStart;
        public event Action<DialogueStatement.Line> OnLine;
        public event Action OnEnd;

        private Dictionary<string, List<TagProcessor>> tagProcessors = new Dictionary<string, List<TagProcessor>>();

        public void BeginDialogue(DialogueProcessor processor)
        {
            OnStart.Invoke();

            this.currentProcessor = processor;
            Next();

        }

        public void Next()
        {
            currentLine = currentProcessor.GetNextLine();
            if (currentLine == null)
            {
                EndDialogue();
                return;
            }


            foreach (KeyValuePair<string, string> kvp in currentLine.Tags)
            {
                CallTagProcessors(kvp.Key, kvp.Value);
            }

            OnLine?.Invoke(currentLine);
        }

        public void EndDialogue()
        {
            currentProcessor = null;
            currentLine = null;

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
