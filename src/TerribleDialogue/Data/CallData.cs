using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue.Data
{
    public readonly struct CallData
    {
        public string Name { get; }
        public object[] Args { get; }

        public CallData(string name, object[] args)
        {
            Name = name;
            Args = args;
        }
    }
}
