using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue.Parser
{
    internal static class TerribleDialogueParser
    {
        internal static TokenListParser<TerribleDialogueToken, string> QuotedText =
            Token.EqualTo(TerribleDialogueToken.QuotedText).Apply(TerribleDialogueTextParsers.QuotedText);

        // Just return the text content of the token since it encapsulates the entire id
        internal static TokenListParser<TerribleDialogueToken, string> Id =
            Token.EqualTo(TerribleDialogueToken.Identifier).Select(t => t.Span.ToStringValue());

    }
}
