using System;
using System.Collections.Generic;
using System.Text;
using HDL.HDLGenerator;

namespace HDL.Compiler
{
    class CompileResult
    {
        public List<Gate> Gates { get; set; }
        public List<Signal> Inputs { get; set; }
        public List<Signal> Outputs { get; set; }
    }
}
