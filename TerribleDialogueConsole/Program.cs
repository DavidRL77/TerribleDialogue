using Davicro.TerribleDialogue;
using Sprache;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dialogueFile = "test.tdlg";
            Random rnd = new Random();

            DialogueObject obj = DialogueGrammar.Dialogue.Parse(File.ReadAllText(Path.Join("Dialogue", dialogueFile)));
            DialogueProcessor processor = new DialogueProcessor(obj, rnd.Next);

            while(!processor.HasEndedDialogue)
            {
                if(processor.HasNextLine())
                {
                    Console.Write(processor.GetNextLine().Text);
                    Console.ReadLine();
                } 
                else
                {
                    processor.EndNode();
                    Console.WriteLine(new String('=', Console.BufferWidth));
                }

            }
        }

    }
}
