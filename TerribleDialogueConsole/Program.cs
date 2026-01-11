using Davicro.TerribleDialogue;
using Sprache;
using System.Text;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        private static readonly Random random = new Random();

        private static readonly Character[] characters = { 
            CreateCharacter("John", "Dialogue/john.tdlg"),
            CreateCharacter("Byte", "Dialogue/byte.tdlg"),
            CreateCharacter("Cute anime girl", "Dialogue/cute.tdlg")
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            while(true)
            {
                Console.WriteLine("Who do you want to talk to?");
                for(int i = 0; i < characters.Length; i++)
                {
                    Console.WriteLine(characters[i].Name);
                }
                Console.Write("> ");

                string answer = Console.ReadLine();
                Character character = characters.FirstOrDefault(c => c.Name.ToLower() == answer.ToLower().Trim());
                if(character == null)
                {
                    Console.WriteLine("No character by that name");
                    Console.WriteLine();
                    continue;
                }

                Console.Clear();
                TalkToCharacter(character);
                Console.Clear();
            }
        }

        private static void TalkToCharacter(Character c)
        {
            if(c.Processor.HasEndedDialogue)
            {
                Console.Write($"{c.Name} has nothing to say.");
                Console.ReadLine();
                return;
            }

            while(c.Processor.HasNextLine()) 
            {
                Console.Write(c.Processor.GetNextLine().Text);
                Console.ReadLine();
            }

            c.Processor.EndNode();
        }

        private static Character CreateCharacter(string name, string dialogueFile)
        {
            return new Character(name, new DialogueProcessor(DialogueGrammar.Dialogue.Parse(File.ReadAllText(dialogueFile)), random.Next));
        }

    }
}
