using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDL.Parser;

namespace HDL.Compiler
{
    class Instance
    {
        public string Name;
        public Module Module;
        public List<Pin> Inputs;
        public List<Pin> Outputs;

        public Instance(List<Token> tokens)
        {
            Name = tokens[0].Value;
            var moduleName = tokens[2].Value;
            Module = Module.Modules.First(x => x.Name == moduleName);
            Inputs = Module.Inputs.Select(x => new Pin(x.Name)).ToList();
            Outputs = Module.Outputs.Select(x => new Pin(x.Name)).ToList();
        }
    }
}
