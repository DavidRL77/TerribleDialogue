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

        [TestCase(@"""quoted text"" : , identifier +12.5 -2 [value=""more quoted text""]", true)]
        [TestCase(@"""incomplete text", false)]
        [TestCase(@"""what is this"" ?", false)]
        [TestCase(@"delimiters are 2imporant", false)]
        public void TokenizerTest(string input, bool shouldTokenize)
        {
            var result = TerribleDialogueTokenizer.Instance.TryTokenize(input);
            TestContext.Out.WriteLine($"Result: {result}");

            Assert.That(result.HasValue, Is.EqualTo(shouldTokenize));
        }

        [TestCase("data/parser_test.tdlg")]
        public void FileTokenizerTest(string file)
        {
            string input = File.ReadAllText(file);
            var result = TerribleDialogueTokenizer.Instance.TryTokenize(input);
            TestContext.Out.WriteLine($"Result: {result}");

            Assert.That(result.HasValue, Is.True);
        }

        [TestCase(@"""This is some text!!!""", "This is some text!!!")]
        [TestCase(@"""Some nasty \r \""escape\"" \\ sequences!!! \n """, "Some nasty \r \"escape\" \\ sequences!!! \n ")]
        public void StringParserTest(string input, string expected) 
        {
            string result = TerribleDialogueTextParsers.QuotedText.Parse(input);
            Assert.That(expected, Is.EqualTo(result));
        }

        [TestCase("12", 12)]
        [TestCase("-12", -12)]
        [TestCase("1.10", 1.1f)]
        [TestCase("-1.10", -1.1f)]
        public void NumericParserTest(string input, float expected)
        {
            float result = TerribleDialogueTextParsers.Number.Parse(input);
            Assert.That(expected, Is.EqualTo(result));
        }
    }
}