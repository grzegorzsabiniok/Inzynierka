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
            Xor,
            Not
        }
        public string Name { get; set; }
        public GateType Type;

        public List<Signal> Signals = new List<Signal>();

        public Gate() { }

        public Gate(Module module, List<Signal> inputs, Signal output, string name)
        {
            if (Enum.TryParse(module.Name, out Type))
            {
                Signals.Add(inputs[0]);
                Signals.Add(inputs[1]);
                Signals.Add(output);
                Name = name;
            }
            else
            {
                Console.WriteLine("wrong gate module");
            }
        }

        public static Gate Create(Module module, List<Signal> inputs, Signal output, string name)
        {
            if (module.Name == "Not")
            {
                return new NotGate(inputs[0], output, name);
            }
            else
            {
                return new Gate(module, inputs, output, name);
            }
        }

    }

    class NotGate : Gate
    {
        public NotGate(Signal input, Signal output, string name)
        {
            Signals.Add(input);
            Signals.Add(output);
            Name = name;
            Type = GateType.Not;
        }
    }
}
