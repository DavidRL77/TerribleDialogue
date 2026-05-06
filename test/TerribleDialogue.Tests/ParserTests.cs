using Superpower;
using Superpower.Parsers;
using TerribleDialogue.Parser;

namespace TerribleDialogue.Tests
{
    internal class ParserTests
    {
        [TestCase(@"""Hello this is a \""quoted\"" \n text!""", true)]
        [TestCase("This text has no quotes!", false)]
        [TestCase("\"Oops, forgot to close the quotes!", false)]
        public void QuotedTextTokenTest(string input, bool shouldParse)
        {
            var result = TerribleDialogueTokenizer.QuotedTextToken.TryParse(input);
            TestContext.Out.WriteLine($"Result: {result}");

            Assert.That(result.HasValue, Is.EqualTo(shouldParse));
        }

        [TestCase(@"""quoted text"" : , [""more quoted text""]", true)]
        [TestCase(@"""incomplete text", false)]
        [TestCase(@"""what is this"" ?", false)]
        public void TokenizerTest(string input, bool shouldTokenize)
        {
            var result = TerribleDialogueTokenizer.Tokenizer.TryTokenize(input);
            TestContext.Out.WriteLine($"Result: {result}");

            Assert.That(result.HasValue, Is.EqualTo(shouldTokenize));
        }
    }
}