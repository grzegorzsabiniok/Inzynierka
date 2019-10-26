using System;
using System.Collections.Generic;
using System.Text;
using HDL.Parser;

namespace HDL.Compiler
{
    class Pin
    {
        public string Name;

        public Pin(string name)
        {
            Name = name;
        }
        public Pin(Token token)
        {
            Name = token.Value;
        }
    }
}
