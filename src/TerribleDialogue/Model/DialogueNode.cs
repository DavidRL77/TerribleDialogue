namespace Davicro.TerribleDialogue.Model
{
    public record DialogueNode
    {
        public string Id { get; }
        public DialogueStatement Root { get; }

        public DialogueNode(string id, DialogueStatement[] statements)
        {
            Id = id;
            Root = new DialogueStatement.Root(statements);
        }
    }
}