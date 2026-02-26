using Davicro.TerribleDialogue;
using Davicro.TerribleDialogue.Model;
using Sprache;
using System.Text;
using TerribleDialogue;

namespace TerribleDialogueConsole
{
    internal class Program
    {

        private static readonly Random random = new Random();
        private static readonly DialogueManager dialogueManager = new DialogueManager();
        private static readonly ConsoleDialogueDisplay display = new ConsoleDialogueDisplay(dialogueManager);
        private static ISoundPlayer soundPlayer = new NetCoreAdioPlayer();

        private static Character activeCharacter;

        private static readonly List<Character> characters = new() {
            CreateCharacter("John", "Dialogue/john.tdlg"),
            CreateCharacter("Byte", "Dialogue/byte.tdlg"),
            CreateCharacter("Cute anime girl", "Dialogue/cute.tdlg"),
            CreateCharacter("Test guy", "Dialogue/test.tdlg"),
            CreateCharacter("Someone", "Dialogue/someone.tdlg"),
            CreateCharacter("I open my eyes", "Dialogue/narrative.tdlg", true)
        };

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            DialogueGrammar.Dialogue.Parse(File.ReadAllText("Dialogue/byte.tdlg"));

            dialogueManager.OnStart += OnDialogueStart;
            dialogueManager.OnStop += OnStop;
            dialogueManager.OnEnd += OnDialogueEnd;
            dialogueManager.AddTagProcessor("music", ProcessMusicTag);
            dialogueManager.AddTagProcessor("sfx", ProcessSfxTag);

            while(true)
            {
                Console.WriteLine("Who do you want to talk to?");
                for(int i = 0; i < characters.Count; i++)
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

        private static void OnStop()
        {
            dialogueManager.EndDialogue();
        }

        private static void OnDialogueEnd()
        {
            Console.Clear();
            soundPlayer.Stop();

            if(activeCharacter.DeleteWhenOver)
            {
                characters.Remove(activeCharacter);
            }
        }

        private static void TalkToCharacter(Character c)
        {
            if(c.Engine.IsDialogueOver)
            {
                Console.Clear();
                Console.Write($"{c.Name} has nothing else to say.");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            activeCharacter = c;

            dialogueManager.BeginDialogue(c.Engine);

            while(dialogueManager.InDialogue)
            {
                Console.ReadLine();
                dialogueManager.Next();
            }

            activeCharacter = null;
        }

        private static void ProcessMusicTag(string key, string value)
        {
            switch(value)
            {
                case "stop":
                    soundPlayer.Stop();
                    break;
                default:
                    soundPlayer.Play(Path.Join("Music", value + ".wav"));
                    break;
            }
        }

        private static void ProcessSfxTag(string key, string value)
        {
            soundPlayer.Play(Path.Join("Sfx", value + ".wav"));
        }

        private static Character CreateCharacter(string name, string dialogueFile, bool deleteWhenOver = false)
        {
            return new Character(name, new DialogueEngine(DialogueGrammar.Dialogue.Parse(
                File.ReadAllText(dialogueFile)),
                random.Next),
                deleteWhenOver);
        }

        private static ConsoleColor ColorByName(string name)
        {
            if(Enum.TryParse(name, true, out ConsoleColor color))
            {
                return color;
            } else
            {
                return ConsoleColor.White;
            }
        }

    }
}
