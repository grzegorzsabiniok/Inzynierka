using System;
using System.Collections.Generic;
using System.Text;
using HDL.Compiler;

namespace HDL.HDLGenerator
{
    class Gate
    {
        public enum GateType
        {
            And,
            Or,
            Xor
        }
        public string Name { get; set; }
        public GateType Type;

        public Signal
            A, B, Y;

        public Gate(Module module, List<Signal> inputs, Signal output, string name)
        {
            if (Enum.TryParse(module.Name, out Type))
            {
                A = inputs[0];
                B = inputs[1];
                Y = output;
                Name = name;
            }
            else
            {
                Console.WriteLine("problem");
            }
        }

    }
}
