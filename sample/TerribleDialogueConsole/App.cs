using System.CommandLine;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using TerribleDialogue;
using TerribleDialogue.Data;
using TerribleDialogueConsole.SoundPlayer;
using TerribleDialogueConsole.View;
using TerribleDialogueConsole.View.Input;

namespace TerribleDialogueConsole
{
    internal class App
    {
        private readonly ISoundPlayer musicPlayer;
        private readonly ISoundPlayer sfxPlayer;
        private readonly ViewStack viewStack = new ViewStack();
        private readonly ConsolePanel dialoguePanel = new ConsolePanel();
        private readonly Keybind[] keybinds;
        private readonly IInputHandler<ConsoleKeyInfo> inputHandler;

        private readonly DialogueManager dialogueManager;
        private DialogueEngine currentEngine;
        private readonly ConsolePrompt linePrompt;

        // Used to locate Music and Sfx files
        private string baseDirectory;

        public App(ISoundPlayer musicPlayer, ISoundPlayer sfxPlayer)
        {
            this.musicPlayer = musicPlayer;
            this.sfxPlayer = sfxPlayer;
            
            baseDirectory = AppContext.BaseDirectory;
            keybinds = [
                new(ConsoleKey.S, ConsoleModifiers.Alt, JumpSet),
                new(ConsoleKey.N, ConsoleModifiers.Alt, JumpNode),
                new(ConsoleKey.Escape, ConsoleModifiers.None, viewStack.Pop),
                new(ConsoleKey.Q, ConsoleModifiers.Alt, () => dialogueManager.EndDialogue())
                ];

            inputHandler = new KeybindConsoleInputHandler(true, keybinds);
            linePrompt = new ConsolePrompt()
            {
                InputHandler = inputHandler,
                OnComplete = s => dialoguePanel.RemoveElement(linePrompt)
            };

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
            string[] sets = currentEngine.DialogueObject.Sets.Keys.ToArray();
            ConsolePanel selectionPanel = new ConsolePanel(
                new ConsoleText("Select set to jump to:", ConsoleColor.White, Console.BackgroundColor),
                new ConsoleMenu<string>()
                {
                    Options = sets,
                    InputHandler = inputHandler,
                    SelectionCallback = (index, option) => { currentEngine.SetSet(option); dialoguePanel.ClearElements(); viewStack.Pop(); },
                    ForegroundColor = ConsoleColor.Gray
                }
            );

            viewStack.Push(selectionPanel);
        }

        private void JumpNode()
        {
            string[] nodes = currentEngine.DialogueObject.Sets[currentEngine.CurrentSetId].Nodes.Keys.ToArray(); // holy shit
            ConsolePanel selectionPanel = new ConsolePanel(
                new ConsoleText("Select node to jump to:", ConsoleColor.White, Console.BackgroundColor),
                new ConsoleMenu<string>()
                {
                    Options = nodes,
                    InputHandler = inputHandler,
                    SelectionCallback = (index, option) => { currentEngine.SetNode(option); dialoguePanel.ClearElements(); viewStack.Pop(); },
                    ForegroundColor = ConsoleColor.Gray
                }
            );

            viewStack.Push(selectionPanel);
        }

        private void DialogueManager_OnLine(LineData lineData)
        {
            string displayType = lineData.Tags.GetValueOrDefault("display", "newline");
            string block = lineData.Tags.GetValueOrDefault("block", "yes");
            string[] splitLines = lineData.Text.Split("<br>");

            ConsoleColor color = ColorByName(lineData.Tags.GetValueOrDefault("color", "white"));

            dialoguePanel.AddElement(new ConsoleText(lineData.Text, color, Console.BackgroundColor, displayType == "newline"));
            dialoguePanel.AddElement(linePrompt);
        }


        private void DialogueManager_OnChoices(string[] choices)
        {
            int choice = ConsoleDisplay.Menu(choices, keybinds);
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
            viewStack.Push(dialoguePanel);
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
            viewStack.Clear();
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
