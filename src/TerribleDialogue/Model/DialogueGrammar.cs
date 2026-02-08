using Sprache;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Davicro.TerribleDialogue.Model
{
    // For future reference: ORDER MATTERS! Parsers need to be defined before they're used or they'll be null.

    public static class DialogueGrammar
    {
        // Every set starts at node 0 by default
        private static FlowAction DEFAULT_SET_FLOW_ACTION = new FlowAction.NodeAction("0");

        /// <summary>
        /// Eat all the whitespace and just return like one whitespace idk
        /// </summary>
        private static readonly Parser<char> OptionalWhitespace =
            Parse.WhiteSpace.Many().Optional().Return(' ');

        /// <summary>
        /// A special character preceded by a backslash that needs to be escaped
        /// </summary>
        private static readonly Parser<char> EscapedChar =
            from backslash in Parse.Char('\\')
            from c in Parse.AnyChar
            select c switch
            {
                'n' => '\n',
                't' => '\t',
                'r' => '\r',
                '\\' => '\\',
                '"' => '"',
                _ => c
            };

        /// <summary>
        /// Text within "quotes"
        /// </summary>
        private static readonly Parser<string> QuotedText =
            from leading in OptionalWhitespace
            from open in Parse.Char('"')
            from content in EscapedChar.Or(Parse.CharExcept('"')).Many().Text()
            from end in Parse.Char('"')
            from newLine in Parse.LineEnd.Optional()
            select content;

        /// <summary>
        /// An id that is composed of only letter, digits or underscores.
        /// Stops at any other character
        /// </summary>
        private static readonly Parser<string> Id =
            from id in Parse.LetterOrDigit.Or(Parse.Char('_')).Many().Text()
            select id;


        /// <summary>
        /// Key=Value
        /// </summary>
        private static readonly Parser<KeyValuePair<string, string>> KeyValue =
            from key in Id
            from sign in Parse.Char('=')
            from value in QuotedText.Or(Id)
            select new KeyValuePair<string, string>(key, value);

        /// <summary>
        /// [Attribute=Value,Attribute=Value]
        /// </summary>
        private static readonly Parser<Dictionary<string, string>> Attributes =
            from open in Parse.Char('[')
            from kvp in KeyValue.DelimitedBy(Parse.Char(','))
            from close in Parse.Char(']')
            select new Dictionary<string, string>(kvp);

        /// <summary>
        /// Quoted text and a dictionary of tags
        /// </summary>
        private static readonly Parser<DialogueStatement.Line> LineStatement =
            from text in QuotedText
            from space in OptionalWhitespace
            from tags in Attributes.Optional()
            select new DialogueStatement.Line(text, tags.GetOrElse(new Dictionary<string, string>()));

        /// <summary>
        /// A redirect node that takes the dialogue to the specified action
        /// </summary>
        private static readonly Parser<DialogueStatement.Goto> GotoStatement =
            from leading in OptionalWhitespace
            from keyword in Parse.String("goto")
            from whiteSpace in Parse.WhiteSpace
            from action in FlowAction
            from now in ( // Grouped like this to account for the space
                from w1 in Parse.WhiteSpace
                from now in Parse.IgnoreCase("now")
                select now

            ).Optional()
            select new DialogueStatement.Goto(action, !now.IsDefined);

        /// <summary>
        /// General statement (line, goto, if...)
        /// </summary>
        private static readonly Parser<DialogueStatement> Statement =
            LineStatement.Or<DialogueStatement>(GotoStatement);


        private static readonly Parser<FlowAction.NodeAction> NodeAction =
            from header in Parse.String("node")
            from id in FlowActionId
            select new FlowAction.NodeAction(id);

        private static readonly Parser<FlowAction.SetAction> SetAction =
            from header in Parse.String("set")
            from id in FlowActionId
            select new FlowAction.SetAction(id, false);

        private static readonly Parser<FlowAction.RandomAction> RandomActionDiscard =
            from header in Parse.String("random")
            select new FlowAction.RandomAction(true);

        private static readonly Parser<FlowAction.RandomAction> RandomAction =
            from header in Parse.String("random")
            select new FlowAction.RandomAction(false);

        private static readonly Parser<FlowAction.PreviousAction> PreviousAction =
            from header in Parse.String("previous")
            select new FlowAction.PreviousAction();

        private static readonly Parser<FlowAction.EndAction> EndAction = 
            from header in Parse.String("END")
            select new FlowAction.EndAction();

        private static readonly Parser<FlowAction> FlowAction =
            NodeAction.Or<FlowAction>(SetAction).Or(RandomActionDiscard).Or(PreviousAction).Or(EndAction);


        private static readonly Parser<string> FlowActionId =
            from colon in Parse.Char(':')
            from id in Id
            select id;

        /// <summary>
        /// A way to specify where a set starts (only set and random action is allowed)
        /// </summary>
        private static readonly Parser<FlowAction> SetFlowAction =
            from leading in OptionalWhitespace
            from start in Parse.String(">>")
            from whiteSpace in OptionalWhitespace
            from action in NodeAction.Or<FlowAction>(RandomAction).Or(SetAction)
            select action;

        /// <summary>
        /// A node that contains an id, bunch of lines of quoted text and an on finished.
        /// Why a key value pair? Because Node doesn't contain an id in itself, but it needs an id to be mapped later into a dictionary
        /// </summary>
        private static readonly Parser<DialogueNode> Node =
            from leading in OptionalWhitespace
            from header in Parse.String("node")
            from space in Parse.WhiteSpace
            from id in Id
            from colon in Parse.Char(':')
            from lineEnd in Parse.LineEnd
            from statements in Statement.AtLeastOnce()
            from nodeEnd in Parse.LineEnd.Many()
            select new DialogueNode(id, statements.ToArray());

        /// <summary>
        /// A dictionary of nodes mapped by its id
        /// </summary>
        private static readonly Parser<Dictionary<string, DialogueNode>> NodeDictionary =
            from nodes in Node.AtLeastOnce()
            select nodes.ToDictionary(node => node.Id, node => node);

        /// <summary>
        /// A set that contains an id and a bunch of nodes.
        /// </summary>
        private static readonly Parser<DialogueSet> Set =
            from header in Parse.String("set")
            from space in Parse.WhiteSpace
            from id in Id
            from colon in Parse.Char(':')
            from flowAction in SetFlowAction.Optional()
            from nodes in NodeDictionary
            from setEnd in Parse.LineEnd.Many().Optional()
            select new DialogueSet(id, nodes, flowAction.GetOrElse(DEFAULT_SET_FLOW_ACTION));

        /// <summary>
        /// A dictionary of sets mapped by its id
        /// </summary>
        private static readonly Parser<Dictionary<string, DialogueSet>> SetDictionary =
            from sets in Set.AtLeastOnce()
            select sets.ToDictionary(set => set.Id, set => set);

        /// <summary>
        /// A bunch of sets
        /// </summary>
        public static readonly Parser<DialogueObject> Dialogue =
            from sets in SetDictionary.End()
            select new DialogueObject(sets);

    }
}
