using Davicro.TerribleDialogue;
using Davicro.TerribleDialogue.Model;
using Sprache;
using System.Text;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        private static readonly Random random = new Random();
        private static readonly DialogueManager dialogueManager = new DialogueManager();
        private static ISoundPlayer soundPlayer = new NetCoreAdioPlayer();

        private static readonly Character[] characters = {
            CreateCharacter("John", "Dialogue/john.tdlg"),
            CreateCharacter("Byte", "Dialogue/byte.tdlg"),
            CreateCharacter("Cute anime girl", "Dialogue/cute.tdlg"),
            CreateCharacter("Color guy", "Dialogue/test.tdlg")
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            dialogueManager.OnStart += OnDialogueStart;
            dialogueManager.OnLine += DisplayLine;
            dialogueManager.OnEnd += OnDialogueEnd;
            dialogueManager.AddTagProcessor("music", ProcessMusicTag);
            dialogueManager.AddTagProcessor("sfx", ProcessSfxTag);

            while (true)
            {
                Console.WriteLine("Who do you want to talk to?");
                for (int i = 0; i < characters.Length; i++)
                {
                    Console.WriteLine(characters[i].Name);
                }
                Console.Write("> ");

                string answer = Console.ReadLine();
                Character character = characters.FirstOrDefault(c => c.Name.ToLower() == answer.ToLower().Trim());
                if (character == null)
                {
                    Console.WriteLine("No character by that name");
                    Console.WriteLine();
                    continue;
                }

                TalkToCharacter(character);
            }
        }

        private static void OnDialogueStart()
        {
            Console.Clear();
        }

        private static void OnDialogueEnd()
        {
            Console.Clear();
            soundPlayer.Stop();
        }

        private static void DisplayLine(string line)
        {
            Console.ForegroundColor = ColorByName(dialogueManager.CurrentTags.GetValueOrDefault("color", "white"));
            Console.Write(line);

            Console.ResetColor();
        }

        private static void TalkToCharacter(Character c)
        {
            if (c.Processor.IsDialogueOver)
            {
                Console.Clear();
                Console.Write($"{c.Name} has nothing else to say.");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            dialogueManager.BeginDialogue(c.Processor);

            while (dialogueManager.InDialogue)
            {
                Console.ReadLine();
                dialogueManager.Next();
            }
        }

        private static void ProcessMusicTag(string key, string value)
        {
            switch(value)
            {
                case "stop":
                    soundPlayer.Stop();
                    break;
                default:
                    soundPlayer.Play(Path.Join("Music", value+".wav"));
                    break;
            }
        }

        private static void ProcessSfxTag(string key, string value)
        {
            soundPlayer.Play(Path.Join("Sfx", value + ".wav"));
        }

        private static Character CreateCharacter(string name, string dialogueFile)
        {
            return new Character(name, new DialogueProcessor(DialogueGrammar.Dialogue.Parse(
                File.ReadAllText(Path.Combine(AppContext.BaseDirectory,dialogueFile))), 
                random.Next));
        }

        private static ConsoleColor ColorByName(string name)
        {
            if(Enum.TryParse(name, true, out ConsoleColor color))
            {
                return color;
            }
            else
            {
                return ConsoleColor.White;
            }
        }

    }
}
