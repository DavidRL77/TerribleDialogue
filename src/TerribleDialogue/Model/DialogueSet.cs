using System.Collections.Generic;

namespace Davicro.TerribleDialogue.Model
{
    public record DialogueSet
    {
        public string Id { get; }
        public Dictionary<string, DialogueNode> Nodes { get; }
        public FlowAction StartFlowAction { get; }

        public DialogueSet(string id, Dictionary<string, DialogueNode> nodes, FlowAction startFlowAction) {
            Id = id;
            Nodes = nodes;
            StartFlowAction = startFlowAction;
        }
    }
}