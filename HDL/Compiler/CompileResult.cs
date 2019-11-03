using System;
using System.Collections.Generic;
using System.Text;
using HDL.HDLGenerator;

namespace HDL.Compiler
{
    class CompileResult
    {
        public List<Gate> Gates;
        public List<Signal> Inputs;
        public List<Signal> Outputs;
    }
}
