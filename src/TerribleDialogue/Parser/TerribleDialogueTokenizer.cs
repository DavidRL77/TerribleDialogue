using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace TerribleDialogue.Parser
{
    internal static class TerribleDialogueTokenizer
    {
        internal static readonly TextParser<Unit> QuotedTextToken =
            from open in Character.EqualTo('"')
            from content in Character.EqualTo('\\') .IgnoreThen(Character.AnyChar).Value(Unit.Value).Try()
            .Or(Character.Except('"').Value(Unit.Value))
            .IgnoreMany()
            from close in Character.EqualTo('"')
            select Unit.Value;
    }
}