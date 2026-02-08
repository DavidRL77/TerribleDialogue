using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue
{
    internal readonly struct Pointer
    {
        public int StatementIndex { get; }
        public int Branch { get; }

        public Pointer(int statementIndex, int branch)
        {
            StatementIndex = statementIndex;
            Branch = branch;
        }

        public Pointer Next() => new Pointer(StatementIndex+1, Branch);
    }
}
