namespace Davicro.TerribleDialogue.Model
{
    public abstract record FlowAction
    {
        public sealed record NodeAction : FlowAction
        {
            public string Id { get; }

            public NodeAction(string id)
            {
                Id = id;
            }
        }

        public sealed record SetAction : FlowAction
        {
            public string Id { get; }
            public bool RandomNode { get; }

            public SetAction(string id, bool randomNode)
            {
                Id = id;
                RandomNode = randomNode;
            }
        }

        public sealed record RandomAction : FlowAction
        {
            public bool Discard { get; }

            public RandomAction(bool discard)
            {
                Discard = discard;
            }
        }

        public sealed record PreviousAction : FlowAction
        {
        }

        public sealed record EndAction : FlowAction
        {
        }
    }
}