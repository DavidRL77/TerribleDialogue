using Superpower.Display;

namespace TerribleDialogue.Parser
{
    internal enum TerribleDialogueToken
    {
        QuotedText,

        [Token(Description="Any letter or underscore")]
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
        Equals
    }
}