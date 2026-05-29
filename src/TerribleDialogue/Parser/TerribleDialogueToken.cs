using Superpower.Display;

namespace TerribleDialogue.Parser
{
    internal enum TerribleDialogueToken
    {
        QuotedText,

        [Token(Description="only letters and underscore")]
        Identifier,

        Number,

        [Token(Example="[")]
        LSquareBracket,

        [Token(Example="]")]
        RSquareBracket,

        [Token(Example=",")]
        Comma,

        [Token(Description = "colon")]
        Colon,

        [Token(Description="semicolon")]
        Semicolon,

        [Token(Example="=")]
        Equals,

        [Token(Example=">>")]
        FlowStart,

        [Token(Example="*")]
        Choice
    }
}