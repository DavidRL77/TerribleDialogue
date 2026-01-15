using Davicro.TerribleDialogue;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue
{
    public class DialogueManager
    {
        public delegate void TagProcessor(string key, string value);

        public bool InDialogue { get => currentProcessor != null; }
        public DialogueLine CurrentLine { get => currentLine; set => currentLine = value; }

        private DialogueProcessor currentProcessor;
        public DialogueLine currentLine;

        public event Action OnStart;
        public event Action<DialogueLine> OnLine;
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
            if(currentProcessor.HasNextLine())
            {
                currentLine = currentProcessor.GetNextLine();

                foreach(KeyValuePair<string,string> kvp in currentLine.Tags)
                {
                    CallTagProcessors(kvp.Key, kvp.Value);
                }

                OnLine?.Invoke(currentLine);
            } 
            else
            {
                currentProcessor.EndNode();
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            currentProcessor = null;
            currentLine = null;

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
