using Superpower.Display;

namespace TerribleDialogue.Parser
{
    internal enum TerribleDialogueToken
    {
        QuotedText,

        [Token(Description="Only letters and underscore allowed")]
        Identifier,

        Number,

        [Token(Example="[")]
        LSquareBracket,

        [Token(Example="]")]
        RSquareBracket,

        [Token(Example=",")]
        Comma,

        [Token(Example=":")]
        Colon,

        [Token(Example="=")]
        Equals,

        [Token(Example=">>")]
        FlowStart,

        [Token(Example="*")]
        Choice
    }
}