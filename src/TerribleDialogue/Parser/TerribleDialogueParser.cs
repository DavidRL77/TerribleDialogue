using Superpower;
using Superpower.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TerribleDialogue.Model;

namespace TerribleDialogue.Parser
{
    public static class TerribleDialogueParser
    {
        // Every set starts at node 0 by default
        private static FlowAction DEFAULT_START_ACTION = new FlowAction.NodeAction("0");

        internal static TokenListParser<TerribleDialogueToken, Superpower.Model.Token<TerribleDialogueToken>> Keyword(string keyword) =>
        Token.EqualToValue(TerribleDialogueToken.Identifier, keyword);

        internal static TokenListParser<TerribleDialogueToken, string> QuotedText =
            Token.EqualTo(TerribleDialogueToken.QuotedText).Apply(TerribleDialogueTextParsers.QuotedText);

        // Just return the text content of the token since it encapsulates the entire id
        internal static TokenListParser<TerribleDialogueToken, string> Id =
            Token.EqualTo(TerribleDialogueToken.Identifier).Select(t => t.Span.ToStringValue());

        internal static TokenListParser<TerribleDialogueToken, string> NumericId =
            Token.EqualTo(TerribleDialogueToken.Number).Select(n => n.Span.ToStringValue());

        // This is needed because I'm fucking stupid and made most of my nodes have numeric ids, which get tokenized as numbers,
        // and I either change every node id to have a proper name, or just support numeric ids as well.
        /// <summary>
        /// Supports text and numeric ids
        /// </summary>
        internal static TokenListParser<TerribleDialogueToken, string> FlexibleId = Id.Or(NumericId);

        internal static TokenListParser<TerribleDialogueToken, float> Number =
        Token.EqualTo(TerribleDialogueToken.Number).Apply(TerribleDialogueTextParsers.Number);

        internal static TokenListParser<TerribleDialogueToken, object> PrimitiveValue =
        QuotedText.Select(o => (object)o)
        .Or(Number.Select(o => (object)o))
        .Or(Id.Select(o => (object)o));

        internal static TokenListParser<TerribleDialogueToken, KeyValuePair<string, string>> KeyValue =
        from key in Id
        from eq in Token.EqualTo(TerribleDialogueToken.Equals)
        from value in QuotedText.Or(Id)
        select KeyValuePair.Create(key, value);

        internal static TokenListParser<TerribleDialogueToken, Dictionary<string, string>> Tags =
        from open in Token.EqualTo(TerribleDialogueToken.LSquareBracket)
        from kvps in KeyValue.AtLeastOnceDelimitedBy(Token.EqualTo(TerribleDialogueToken.Comma))
        from close in Token.EqualTo(TerribleDialogueToken.RSquareBracket)
        select new Dictionary<string, string>(kvps);

        internal static TokenListParser<TerribleDialogueToken, (string, DialogueStatement[])> SingleChoice =
        from delimiter in Token.EqualTo(TerribleDialogueToken.Choice)
        from text in QuotedText
        from statements in Superpower.Parse.Ref(() => Statement).Many()
        select (text, statements);

        internal static TokenListParser<TerribleDialogueToken, FlowAction> EndAction =
        from keyword in Keyword("END")
        select (FlowAction)new FlowAction.EndAction();

        internal static TokenListParser<TerribleDialogueToken, FlowAction> RandomAction =
        from keyword in Keyword("random")
        select (FlowAction)new FlowAction.RandomAction(true);

        internal static TokenListParser<TerribleDialogueToken, FlowAction> PreviousAction =
        from keyword in Keyword("previous")
        select (FlowAction)new FlowAction.PreviousAction();

        internal static TokenListParser<TerribleDialogueToken, FlowAction> NodeAction =
        from keyword in Keyword("node")
        from colon in Token.EqualTo(TerribleDialogueToken.Colon)
        from id in FlexibleId
        select (FlowAction)new FlowAction.NodeAction(id);

        internal static TokenListParser<TerribleDialogueToken, FlowAction> SetAction =
        from keyword in Keyword("set")
        from colon in Token.EqualTo(TerribleDialogueToken.Colon)
        from id in FlexibleId
        select (FlowAction)new FlowAction.SetAction(id);

        internal static TokenListParser<TerribleDialogueToken, FlowAction> FlowAction =
        NodeAction.Or(SetAction).Or(RandomAction).Or(PreviousAction).Or(EndAction);

        internal static TokenListParser<TerribleDialogueToken, DialogueStatement> LineStatement =
        from text in QuotedText
        from tags in Tags.OptionalOrDefault()
        select (DialogueStatement)new DialogueStatement.Line(text, tags ?? new());

        internal static TokenListParser<TerribleDialogueToken, DialogueStatement> GotoStatement =
        from keyword in Keyword("goto")
        from flowAction in FlowAction
        from @break in Keyword("break").Optional()
        select (DialogueStatement)new DialogueStatement.Goto(flowAction, @break.HasValue);

        internal static TokenListParser<TerribleDialogueToken, DialogueStatement> ChoiceStatement =
        from open in Keyword("choice")
        from choices in SingleChoice.AtLeastOnce()
        from close in Keyword("endchoice")
        select (DialogueStatement)new DialogueStatement.Choice( // TODO: Improve choice parsing
            choices.Select(c => c.Item1).ToArray(), 
            choices.Select(c => c.Item2).ToArray());

        internal static TokenListParser<TerribleDialogueToken, DialogueStatement> CallStatement =
        from keyword in Keyword("call")
        from callName in Id
        from args in PrimitiveValue.Many()
        from close in Keyword("endcall")
        select (DialogueStatement)new DialogueStatement.Call(callName, args);

        internal static TokenListParser<TerribleDialogueToken, DialogueStatement> Statement =
        LineStatement.Or(GotoStatement).Or(ChoiceStatement).Or(CallStatement);

        internal static TokenListParser<TerribleDialogueToken, DialogueNode> Node =
        from keyword in Keyword("node")
        from id in FlexibleId
        from colon in Token.EqualTo(TerribleDialogueToken.Colon)
        from statements in Statement.AtLeastOnce()
        select new DialogueNode(id, statements);

        internal static TokenListParser<TerribleDialogueToken, DialogueSet> Set =
        from keyword in Keyword("set")
        from id in FlexibleId
        from colon in Token.EqualTo(TerribleDialogueToken.Colon)
        from flowAction in Token.EqualTo(TerribleDialogueToken.FlowStart).IgnoreThen(FlowAction).OptionalOrDefault(DEFAULT_START_ACTION)
        from nodes in Node.Many()
        select new DialogueSet(id, nodes.ToDictionary(n => n.Id), flowAction);

        internal static TokenListParser<TerribleDialogueToken, DialogueObject> DialogueObject =
        from sets in Set.AtLeastOnce().AtEnd()
        select new DialogueObject(sets.ToDictionary(s => s.Id));

        public static bool TryParse(string input, out DialogueObject value)
        {
            var tokens = TerribleDialogueTokenizer.Instance.TryTokenize(input);
            if(!tokens.HasValue)
            {
                value = null;
                return false;
            }

            var parsed = DialogueObject.TryParse(tokens.Value);
            if(!parsed.HasValue)
            {
                value = null;
                return false;
            }

            value = parsed.Value;
            return true;
        }

        public static DialogueObject Parse(string input)
        {
            var tokens = TerribleDialogueTokenizer.Instance.Tokenize(input);
            return DialogueObject.Parse(tokens);
        }

    }
}
