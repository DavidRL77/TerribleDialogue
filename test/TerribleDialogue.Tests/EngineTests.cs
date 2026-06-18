using System.Text.Json;
using TerribleDialogue;
using TerribleDialogue.Model;

namespace TerribleDialogue.Tests
{
    internal class EngineTests
    {
        private DialogueObject dialogueObject;
        private Random rnd;

        [SetUp]
        public void Setup()
        {
            dialogueObject = new DialogueObject(new()
            {
                { "default", new DialogueSet("default", new()
                {
                    {"0", new DialogueNode("0", [
                        new DialogueStatement.Line("Hello!", new()),
                        new DialogueStatement.Line("This is just a test object for unit testing purposes!", new()),
                        new DialogueStatement.Call("test", ["arg1", 2]),
                        new DialogueStatement.Line("Making dialogue like this sucks, never do this.", new()),
                        new DialogueStatement.Goto(new FlowAction.NodeAction("0"), false)
                        ])}
                },
                new FlowAction.NodeAction("0"))}
            });

            rnd = new Random(0);
        }

        [Test]
        public void SerializationTest()
        {
            DialogueEngine engine = new DialogueEngine(this.dialogueObject, this.rnd.Next);
            engine.Step();
            engine.Step();
            engine.Step();

            DialogueState stateBeforeLoad = engine.State;
            string savedJson = JsonSerializer.Serialize(engine.State, new JsonSerializerOptions() { WriteIndented = true });
            TestContext.Out.WriteLine(savedJson);

            TestContext.Out.WriteLine("=================");

            DialogueState? loadedState = JsonSerializer.Deserialize<DialogueState>(savedJson);
            string loadedJson = JsonSerializer.Serialize(loadedState, new JsonSerializerOptions() { WriteIndented = true });
            TestContext.Out.WriteLine(loadedJson);

            Assert.That(loadedJson, Is.EqualTo(savedJson));

        }
    }
}