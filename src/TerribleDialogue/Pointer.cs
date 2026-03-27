namespace TerribleDialogue
{
    public struct Pointer
    {
        public static Pointer BranchStart(int branch) => new Pointer(-1, branch);

        public int StatementIndex { get; set; }
        public int Branch { get; set; }

        public Pointer(int statementIndex, int branch)
        {
            this.StatementIndex = statementIndex;
            this.Branch = branch;
        }

        public Pointer Next() => new Pointer(StatementIndex + 1, Branch);
    }
}
