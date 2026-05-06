using Superpower;
using TerribleDialogue.Parser;

namespace TerribleDialogue.Tests
{
    internal class ParserTests
    {
        [TestCase("\"Hello this is a \"quoted\" \n text!\"", true)]
        [TestCase("This text has no quotes!", false)]
        [TestCase("\"Oops, forgot to close the quotes!", false)]
        public void QuotedTextTokenTest(string input, bool shouldParse)
        {
            var result = TerribleDialogueTokenizer.QuotedTextToken.TryParse(input);
            if(!result.HasValue)
                TestContext.Out.WriteLine($"Error message: {result}");

            Assert.That(shouldParse, Is.EqualTo(result.HasValue));
        }
    }
}