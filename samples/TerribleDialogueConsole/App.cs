using Sprache;
using System.Text;
using System.Text.Json;
using TerribleDialogue;
using TerribleDialogue.Data;
using TerribleDialogue.Model;
using TerribleDialogueConsole.SoundPlayer;
using TerribleDialogueConsole.View;

namespace TerribleDialogueConsole
{
    internal class App
    {
        private static string SavePath => Path.Combine(AppContext.BaseDirectory, "saves");

        private readonly ISoundPlayer musicPlayer;
        private readonly ISoundPlayer sfxPlayer;
        private readonly IDialogueView view;

        private readonly Random random;
        private readonly DialogueManager dialogueManager;
        private readonly List<Character> characters;

        private Character activeCharacter;
        private string activeMusic = null;

        public App(ISoundPlayer musicPlayer, ISoundPlayer sfxPlayer)
        {
            this.musicPlayer = musicPlayer;
            this.sfxPlayer = sfxPlayer;
            this.view = new ConsoleDialogueView(InputHandler);

            random = new Random();
            dialogueManager = new DialogueManager();

            dialogueManager.OnLine += DialogueManager_OnLine;
            dialogueManager.OnChoices += DialogueManager_OnChoices;
            dialogueManager.OnStart += OnDialogueStart;
            dialogueManager.OnStop += OnStop;
            dialogueManager.OnEnd += OnDialogueEnd;
            dialogueManager.AddCallHandler("play", PlayCallHandler);
            dialogueManager.AddCallHandler("stop", StopCallHandler);
            dialogueManager.AddCallHandler("screen", ScreenCallHandler);
            dialogueManager.AddCallHandler("wait", WaitCallHandler);

            characters = new() {
                CreateCharacter("John", "Dialogue/john.tdlg"),
                CreateCharacter("Byte", "Dialogue/byte.tdlg"),
                CreateCharacter("Cute anime girl", "Dialogue/cute.tdlg"),
                CreateCharacter("Test guy", "Dialogue/test.tdlg"),
                CreateCharacter("Someone", "Dialogue/someone.tdlg"),
                CreateCharacter("I open my eyes", "Dialogue/narrative.tdlg", true)
            };

            foreach(Character character in characters)
            {
                string saveFile = GetCharacterSavePath(character);
                if(File.Exists(saveFile))
                {
                    DialogueState state = JsonSerializer.Deserialize<DialogueState>(File.ReadAllText(saveFile));
                    character.Engine.LoadState(state);
                }
            }
        }

        private bool InputHandler(ConsoleKeyInfo keyInfo)
        {
            // ALT + S = Save
            if(keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt) && keyInfo.Key == ConsoleKey.S)
            {
                SaveCharacter(activeCharacter);
                return true;
            }

            // ALT + D = Delete save
            if(keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt) && keyInfo.Key == ConsoleKey.D && activeCharacter != null)
            {
                string saveFile = GetCharacterSavePath(activeCharacter);
                if(File.Exists(saveFile))
                {
                    File.Delete(saveFile);
                }
            }



            return false;
        }

        private void DialogueManager_OnLine(LineData lineData)
        {
            view.DisplayLine(lineData);
        }

        private void DialogueManager_OnChoices(string[] choices)
        {
            int choice = view.DisplayChoices(choices);
            if(choice < 0)
                return;

            dialogueManager.AddChoice(choice);
            dialogueManager.Next();
        }

        public void Run()
        {
            Console.Clear();
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
                dialogueManager.Next();
            }

            activeCharacter = null;
        }

        private void ScreenCallHandler(CallData callData)
        {
            string action = callData.Args.Get<string>(0);

            if(action == "clear")
                Console.Clear();
        }

        private void PlayCallHandler(CallData callData)
        {
            string channel = callData.Args.GetOrDefault<string>(0);
            string audioFile = callData.Args.GetOrDefault<string>(1);

            if(channel == null || audioFile == null)
                return;

            ISoundPlayer soundPlayer;
            bool loop;
            string folder;
            switch(channel)
            {
                case "sfx":
                    folder = "Sfx";
                    soundPlayer = sfxPlayer;
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


            string filePath = Path.Combine(AppContext.BaseDirectory, folder, audioFile);

            if(loop)
                soundPlayer.PlayLooping(filePath);
            else
                soundPlayer.Play(filePath);
        }

        private void StopCallHandler(CallData callData)
        {
            string channel = callData.Args.GetOrDefault<string>(0);
            
            if(channel == null)
                return;

            ISoundPlayer soundPlayer = channel switch
            {
                "sfx" => sfxPlayer,
                "music" => musicPlayer,
                _ => null
            };
            soundPlayer.Stop();
        }

        private void WaitCallHandler(CallData callData)
        {
            float seconds = callData.Args.GetOrDefault<float>(0);
            Thread.Sleep((int)(seconds*1000));
        }

        private static void SaveCharacter(Character character)
        {
            if(character == null)
                return;

            string saveFile = GetCharacterSavePath(character);
            Directory.CreateDirectory(SavePath);

            string stateJson = JsonSerializer.Serialize(character.Engine.State);
            File.WriteAllText(saveFile, stateJson);
        }

        private static string GetCharacterSavePath(Character character) => Path.Combine(SavePath, character.Name);

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
