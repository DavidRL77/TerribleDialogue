namespace TerribleDialogue
{
    internal readonly struct Pointer
    {
        public static Pointer BranchStart(int branch) => new Pointer(-1, branch);

        public int StatementIndex { get; }
        public int Branch { get; }

        public Pointer(int statementIndex, int branch) {
            StatementIndex = statementIndex;
            Branch = branch;
        }

        public Pointer Next() => new Pointer(StatementIndex + 1, Branch);
    }
}
