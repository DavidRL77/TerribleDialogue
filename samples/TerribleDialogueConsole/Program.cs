using Davicro.TerribleDialogue;
using Sprache;
using System.Media;
using System.Text;
using TerribleDialogue;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        private static readonly Random random = new Random();
        private static readonly DialogueManager dialogueManager = new DialogueManager();
        private static readonly SoundPlayer soundPlayer = new SoundPlayer();

        private static readonly Character[] characters = { 
            CreateCharacter("John", "Dialogue/john.tdlg"),
            CreateCharacter("Byte", "Dialogue/byte.tdlg"),
            CreateCharacter("Cute anime girl", "Dialogue/cute.tdlg")
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            dialogueManager.OnStart += OnDialogueStart;
            dialogueManager.OnLine += DisplayLine;
            dialogueManager.OnEnd += OnDialogueEnd;
            dialogueManager.AddTagProcessor("music", ProcessMusicTag);
            dialogueManager.AddTagProcessor("sfx", ProcessSfxTag);

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
            Console.Write(line);
        }

        private static void TalkToCharacter(Character c)
        {
            if(c.Processor.HasEndedDialogue)
            {
                Console.Clear();
                Console.Write($"{c.Name} has nothing else to say.");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            dialogueManager.BeginDialogue(c.Processor);

            while(dialogueManager.InDialogue)
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
                    soundPlayer.SoundLocation = Path.Join("Music", value+".wav");
                    soundPlayer.PlayLooping();
                    break;
            }
        }

        private static void ProcessSfxTag(string key, string value)
        {
            soundPlayer.SoundLocation = Path.Join("Sfx", value + ".wav");
            soundPlayer.Play();
        }

        private static Character CreateCharacter(string name, string dialogueFile)
        {
            return new Character(name, new DialogueProcessor(DialogueGrammar.Dialogue.Parse(File.ReadAllText(dialogueFile)), random.Next));
        }

    }
}
