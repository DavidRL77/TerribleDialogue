using Davicro.TerribleDialogue;
using Davicro.TerribleDialogue.Model;
using Sprache;
using System.Text;
using TerribleDialogue;
using TerribleDialogueConsole.SoundPlayer;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        private readonly Random random;
        private readonly DialogueManager dialogueManager;
        private readonly ConsoleDialogueDisplay display;
        private readonly ISoundPlayer soundPlayer;
        private readonly List<Character> characters;

        private Character activeCharacter;
        private string activeMusic = null;

        static void Main(string[] args)
        {
            // Loading libvlc takes a while
            Console.WriteLine("Loading...");
            using(var player = new LibVLCAudioPlayer())
            {
                Console.Clear();
                Program p = new Program(player);
                p.Run();
            }
        }

        public Program(ISoundPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;

            random = new Random();
            dialogueManager = new DialogueManager();
            display = new ConsoleDialogueDisplay(dialogueManager);

            characters = new() {
                CreateCharacter("John", "Dialogue/john.tdlg"),
                CreateCharacter("Byte", "Dialogue/byte.tdlg"),
                CreateCharacter("Cute anime girl", "Dialogue/cute.tdlg"),
                CreateCharacter("Test guy", "Dialogue/test.tdlg"),
                CreateCharacter("Someone", "Dialogue/someone.tdlg"),
                CreateCharacter("I open my eyes", "Dialogue/narrative.tdlg", true)
            };
        }

        public void Run()
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

        private void OnDialogueStart()
        {
            Console.Clear();
        }

        private void OnStop()
        {
            dialogueManager.EndDialogue();
        }

        private void OnDialogueEnd()
        {
            Console.Clear();
            soundPlayer.Stop();
            activeMusic = null;

            if(activeCharacter.DeleteWhenOver)
            {
                characters.Remove(activeCharacter);
            }
        }

        private void TalkToCharacter(Character c)
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
                if(Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    dialogueManager.Next();
                }
            }

            activeCharacter = null;
        }

        private void ProcessMusicTag(string key, string value)
        {
            if(value == "stop")
            {
                soundPlayer.Stop();
                activeMusic = null;
                return;
            }

            if(value == activeMusic)
            {
                return;
            }

            soundPlayer.PlayLooping(Path.Join(AppContext.BaseDirectory, "Music", value + ".wav"));
            activeMusic = value;
        }

        private void ProcessSfxTag(string key, string value)
        {
            soundPlayer.Play(Path.Join("Sfx", value + ".wav"));
        }

        private Character CreateCharacter(string name, string dialogueFile, bool deleteWhenOver = false)
        {
            return new Character(name, new DialogueEngine(DialogueGrammar.Dialogue.Parse(
                File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory,dialogueFile))),
                random.Next),
                deleteWhenOver);
        }

        private ConsoleColor ColorByName(string name)
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
