using System;
using System.Collections.Generic;
using System.Text;

namespace TerribleDialogue
{
    public class ArgParser
    {
        public int Count => args.Length;

        private object[] args;

        public ArgParser(object[] args)
        {
            this.args = args;
        }

        public T Get<T>(int index)
        {
            return (T)args[index];
        }

        public T GetOrDefault<T>(int index, T @default)
        {
            if(index >= args.Length)
                return @default;

            object obj = args[index];
            return obj is T ? (T)obj : @default;
        }

        public T GetOrDefault<T>(int index) => GetOrDefault(index, default(T));
    }
}
