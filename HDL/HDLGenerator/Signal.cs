using System;
using System.Collections.Generic;
using System.Text;
using HDL.Compiler;

namespace HDL.HDLGenerator
{
    class Signal
    {
        public string Name = "null";

        public Signal() { }
        public Signal(string name)
        {
            Name = name;
            Console.WriteLine(name);
        }
    }
}
