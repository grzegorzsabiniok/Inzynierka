using System;
using System.Collections.Generic;
using System.Text;

namespace HDL.Compiler
{
    class Link
    {
        public List<Pin> Pins;

        public Link(params Pin[] pins)
        {
            Pins = new List<Pin>(pins);
        }
    }
}
