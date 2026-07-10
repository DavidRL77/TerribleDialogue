using System.Text;
using System.Text.Json;
using TerribleDialogue;
using TerribleDialogue.Data;
using TerribleDialogueConsole.SoundPlayer;
using TerribleDialogueConsole.View;

namespace TerribleDialogueConsole
{
    internal class App
    {
        private readonly ISoundPlayer musicPlayer;
        private readonly ISoundPlayer sfxPlayer;
        private readonly IDialogueView view;

        private readonly DialogueManager dialogueManager;
        private DialogueEngine currentEngine;

        // Used to locate Music and Sfx files
        private string baseDirectory;

        public App(ISoundPlayer musicPlayer, ISoundPlayer sfxPlayer)
        {
            this.musicPlayer = musicPlayer;
            this.sfxPlayer = sfxPlayer;
            this.baseDirectory = AppContext.BaseDirectory;
            this.view = new ConsoleDialogueView([
                new(ConsoleKey.S, ConsoleModifiers.Alt, JumpSet)
                ]);

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
        }

        private void JumpSet()
        {
            Console.Clear();
            Console.WriteLine("Set to jump to: ");
            string[] sets = currentEngine.DialogueObject.Sets.Keys.ToArray();
            string set = sets[ConsoleDisplay.Menu(sets)];
            currentEngine.SetSet(set);
            Console.Clear();
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

        public void Run(DialogueEngine engine, string baseDirectory)
        {
            this.baseDirectory = baseDirectory;

            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;

            if(engine.IsDialogueOver)
            {
                Console.Clear();
                Console.Write($"Dialogue is over.");
                Console.ReadLine();
                Console.Clear();
                return;
            }

            currentEngine = engine;
            dialogueManager.BeginDialogue(engine);


            while(dialogueManager.InDialogue)
            {
                dialogueManager.Next();
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
            Console.ResetColor();
            musicPlayer.Stop();
            currentEngine = null;
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


            string filePath = Path.Combine(baseDirectory, folder, audioFile);
            if(!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File not found: '{filePath}'");
                return;
            }

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

        //private static void SaveCharacter(Character character)
        //{
        //    if(character == null)
        //        return;

        //    string saveFile = GetCharacterSavePath(character);
        //    Directory.CreateDirectory(SavePath);

        //    string stateJson = JsonSerializer.Serialize(character.Engine.State);
        //    File.WriteAllText(saveFile, stateJson);
        //}

        //private static string GetCharacterSavePath(Character character) => Path.Combine(SavePath, character.Name);
    }
}
