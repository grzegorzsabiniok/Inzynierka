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
        public Module Module, ParentModule;
        public List<Pin> Inputs;
        public List<Pin> Outputs;
        private List<Token> tokens;

        public Instance() { }
        public Instance(List<Token> tokens, Module parent)
        {
            ParentModule = parent;
            this.tokens = tokens;
            Name = tokens[0].Value;
            var moduleName = tokens[2].Value;
            Module = Module.Modules.First(x => x.Name == moduleName);
            Inputs = Module.Inputs.Select(x => new Pin(x.Name)).ToList();
            Outputs = Module.Outputs.Select(x => new Pin(x.Name)).ToList();
        }

        public virtual void Process()
        {

            tokens.RemoveRange(0, 4);
            tokens.Remove(tokens.Last());

            var splitedByComa = tokens.Aggregate(new List<List<Token>> { new List<Token>() },
                (list, v) =>
                {
                    if (v.Value == ",")
                    {
                        list.Add(new List<Token>());
                    }
                    else
                    {
                        list.Last().Add(v);
                    }
                    return list;
                });
            int i = 0;
            foreach (var argument in splitedByComa)
            {
                var nameTokens = argument.Where(x => x.Type == Token.TokenType.Identifier).ToList();

                Link link = null;

                if (argument.Any(x => x.Value == "."))
                {
                    var gate = ParentModule.Gates.First(x => x.Name == nameTokens[0].Value);

                    link = new Link(Inputs[i], gate.Outputs.First(x => x.Name == nameTokens[1].Value));
                }
                else
                {
                    link = new Link(Inputs[i], ParentModule.Inputs.First(x => x.Name == nameTokens[0].Value));
                }
                ParentModule.Links.Add(link);

                if (argument.First().Value == "!")
                {
                    var negationGate = new Negation(Inputs[i]);

                    var pin = new Pin(Inputs[i].Name + "_s");
                    ParentModule.Links.Add(new Link(
                        negationGate.Outputs[0],
                        pin
                    ));

                    ParentModule.Gates.Add(negationGate);
                    Inputs[i] = pin;
                }

                i++;
            }

        }
    }

    class Negation : Instance
    {
        private static int count = 0;
        public Negation(Pin input)
        {
            Inputs = new List<Pin>() { input };
            Outputs = new List<Pin>() { new Pin(input.Name + "_N") };
            Name = $"Negation_{count++}";
            Module = Module.Modules.First(x => x.Name == "Not");

        }

        public override void Process()
        {
        }
    }
}
