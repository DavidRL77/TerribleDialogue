using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue.Parser
{
    // We need to map string representations of tokens into their primitive values,
    internal static class TerribleDialogueTextParsers
    {
        internal static readonly TextParser<string> String =
            from open in Character.EqualTo('"')
            from content in Character.ExceptIn('"', '\\')
            .Or(Character.EqualTo('\\').IgnoreThen(
                Character.EqualTo('\\')
                .Or(Character.EqualTo('"'))
                .Or(Character.EqualTo('b').Value('\b')
                .Or(Character.EqualTo('f').Value('\f'))
                .Or(Character.EqualTo('n').Value('\n'))
                .Or(Character.EqualTo('r').Value('\r'))
                .Or(Character.EqualTo('t').Value('\t'))
                .Or(Character.EqualTo('u').IgnoreThen( // Do we really need this?
                        Span.MatchedBy(Character.HexDigit.Repeat(4))
                            .Apply(Numerics.HexDigitsUInt32)
                            .Select(cc => (char)cc))))

                ).Named("escape sequence"))
            .Many()
            from close in Character.EqualTo('"')
            select new string(content);

        // DO NOT LEAVE IT LIKE THIS!!!
        // float should not be assumed to be the preferred numeric value,
        // ideally we could return the accurate type based on if it's decimal or not
        // OR specify the numeric type to use in the actual engine
        internal static readonly TextParser<float> Number =
            from number in Numerics.Decimal.Or(Numerics.Integer)
            select float.Parse(number.ToStringValue());
    }
}
