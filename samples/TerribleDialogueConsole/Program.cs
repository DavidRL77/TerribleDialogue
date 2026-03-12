using Sprache;
using System.Text;
using System.Threading.Channels;
using TerribleDialogue;
using TerribleDialogue.Model;
using TerribleDialogueConsole.SoundPlayer;

namespace TerribleDialogueConsole
{
    internal class Program
    {
        private readonly Random random;
        private readonly DialogueManager dialogueManager;
        private readonly ConsoleDialogueDisplay display;
        private readonly ISoundPlayer musicPlayer;
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
            this.musicPlayer = soundPlayer;

            random = new Random();
            dialogueManager = new DialogueManager();
            display = new ConsoleDialogueDisplay(dialogueManager);

            dialogueManager.OnStart += OnDialogueStart;
            dialogueManager.OnStop += OnStop;
            dialogueManager.OnEnd += OnDialogueEnd;
            dialogueManager.AddCallHandler("play", PlayCallHandler);
            dialogueManager.AddCallHandler("stop", StopCallHandler);
            dialogueManager.AddCallHandler("screen", ScreenCallHandler);

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
            musicPlayer.Stop();
            activeMusic = null;

            if(activeCharacter.DeleteWhenOver && activeCharacter.Engine.IsDialogueOver)
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
                // Remember that ConsoleDialogueDisplay handles blocking the input
                // Should be reworked
                dialogueManager.Next();
            }

            activeCharacter = null;
        }

        private void ScreenCallHandler(string name, object[] args)
        {
            string action = args[0] as string;

            if(action == "clear")
                Console.Clear();
        }

        private void PlayCallHandler(string name, object[] args)
        {
            string channel = args[0] as string;
            string audioFile = args[1] as string;

            if(channel == null || audioFile == null)
                return;

            ISoundPlayer soundPlayer;
            bool loop;
            string folder;
            switch(channel)
            {
                case "sfx":
                    folder = "Sfx";
                    soundPlayer = musicPlayer;
                    loop = false;
                    break;
                case "music":
                    folder = "Music";
                    soundPlayer = musicPlayer;
                    loop = true;
                    break;
                default:
                    return;
            }


            string filePath = Path.Combine(AppContext.BaseDirectory, folder, audioFile + ".wav");

            if(loop)
                soundPlayer.PlayLooping(filePath);
            else
                soundPlayer.Play(filePath);
        }

        private void StopCallHandler(string name, object[] args)
        {
            string channel = args[0] as string;
            
            if(channel == null)
                return;

            ISoundPlayer soundPlayer = channel switch
            {
                "sfx" => musicPlayer, // TODO: Change to different player
                "music" => musicPlayer,
                _ => null
            };
            soundPlayer.Stop();
        }

        private Character CreateCharacter(string name, string dialogueFile, bool deleteWhenOver = false)
        {
            return new Character(name, new DialogueEngine(DialogueGrammar.Dialogue.Parse(
                File.ReadAllText(
                    Path.Combine(AppContext.BaseDirectory,dialogueFile))),
                random.Next),
                deleteWhenOver);
        }

    }
}
