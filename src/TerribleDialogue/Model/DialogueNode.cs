namespace Davicro.TerribleDialogue.Model
{
    public record DialogueNode
    {
        public string Id { get; }
        public DialogueStatement[] Statements { get; }

        public DialogueNode(string id, DialogueStatement[] statements)
        {
            Id = id;
            Statements = statements;
        }
    }
}