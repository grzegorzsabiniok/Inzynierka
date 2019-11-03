using System;
using System.Collections.Generic;
using System.Linq;
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

        public Link(Link link)
        {
            Pins = link.Pins.ToList();
        }
    }
}
