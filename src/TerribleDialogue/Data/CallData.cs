namespace TerribleDialogue.Data
{
    public class CallData
    {
        public string Name { get; }
        public ArgParser Args { get; }

        public CallData(string name, ArgParser args)
        {
            Name = name;
            Args = args;
        }
    }
}
