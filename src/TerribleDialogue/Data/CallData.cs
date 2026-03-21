using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue.Data
{
    public readonly struct CallData
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
