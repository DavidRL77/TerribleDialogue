using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace TerribleDialogue.Parser
{
    internal static class TerribleDialogueTokenizer
    {
        /// <summary>
        /// Matches any character in between quotes (") and ignores any character preceded by a backslash (\).
        /// <para>ONLY matches, does not return anything.</para>
        /// </summary>
        /// The reason why we use a Unit parser instead of returning the content, is that we want this parser to be as permissive as possible,
        /// and don't want to actually allocate any string result until later on in the parsing process.
        internal static readonly TextParser<Unit> QuotedTextToken =
            from open in Character.EqualTo('"')
            from content in Character.EqualTo('\\') .IgnoreThen(Character.AnyChar).Value(Unit.Value).Try()
            .Or(Character.Except('"').Value(Unit.Value))
            .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;

        public static readonly Tokenizer<TerribleDialogueToken> Tokenizer =
            new TokenizerBuilder<TerribleDialogueToken>()
            .Ignore(Span.WhiteSpace)
            .Match(Character.EqualTo('['), TerribleDialogueToken.LSquareBracket)
            .Match(Character.EqualTo(']'), TerribleDialogueToken.RSquareBracket)
            .Match(Character.EqualTo(':'), TerribleDialogueToken.Colon)
            .Match(Character.EqualTo(','), TerribleDialogueToken.Comma)
            .Match(Character.EqualTo('='), TerribleDialogueToken.Equals)
            .Match(QuotedTextToken, TerribleDialogueToken.QuotedText)
            .Match(Identifier.CStyle, TerribleDialogueToken.Identifier)
            .Build();
    }
}