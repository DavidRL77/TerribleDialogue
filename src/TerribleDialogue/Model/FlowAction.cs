namespace TerribleDialogue.Model
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

            public SetAction(string id)
            {
                Id = id;
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