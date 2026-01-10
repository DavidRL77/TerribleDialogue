namespace Davicro.TerribleDialogue
{
    public record DialogueNode
    {
        public string Id { get; }
        public DialogueLine[] Lines { get; }
        public FlowAction FlowActionOnEnd { get; }

        public DialogueNode(string id, DialogueLine[] lines, FlowAction flowAction)
        {
            Id = id;
            Lines = lines;
            FlowActionOnEnd = flowAction;
        }
    }
}