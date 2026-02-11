using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue
{
    internal class Pointer
    {
        public int StatementIndex { get; private set;}
        public int Branch { get; }

        public Pointer(int statementIndex, int branch)
        {
            StatementIndex = statementIndex;
            Branch = branch;
        }

        public void Next()
        {
            StatementIndex++;
        }

        public void Previous()
        {
            StatementIndex--;
        }
    }
}
