using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDL.HDLGenerator;
using HDL.Parser;

namespace HDL.Compiler
{
    class Module
    {
        public string Name;
        public List<Pin>
            Inputs,
            Outputs;

        public List<Link> Links = new List<Link>();

        public List<Instance> Gates = new List<Instance>();

        public bool IsAtomic = false;

        private List<Token> tokens;

        public static List<Module> Modules = new List<Module>();

        public Module(List<Token> tokens)
        {
            this.tokens = tokens;
            Name = this.tokens[0].Value;
            this.tokens.RemoveAt(0);
            this.tokens.Remove(this.tokens.Last());
            Modules.Add(this);
        }

        public Module(string name)
        {
            Name = name;
            Modules.Add(this);
        }

        public void Process()
        {
            TakeArguments();
            tokens.RemoveAt(0);

            ProcessInstructions();
            Gates.ToList().ForEach(x => x.Process());
        }

        private void ProcessInstructions()
        {
            var instuctionTokens = new List<Token>();

            foreach (var token in tokens.ToArray())
            {
                instuctionTokens.Add(token);
                tokens.Remove(token);
                if (token.Value == ";")
                {
                    ProcessInstruction(instuctionTokens);
                    instuctionTokens = new List<Token>();
                }
            }
        }

        private void ProcessInstruction(List<Token> tokens)
        {
            if (tokens.Any(x => x.Value == "<="))
            {
                ProcessOutput(tokens);
            }
            else
            {
                ProcessGate(tokens);
            }
        }

        private void ProcessGate(List<Token> tokens)
        {
            var gatesTokens = new List<Token>();
            int bracketsDepth = 0;
            tokens.RemoveAt(0);
            foreach (var token in tokens.ToArray())
            {
                gatesTokens.Add(token);
                if (token.Value == "(")
                    bracketsDepth++;
                if (token.Value == ")")
                    bracketsDepth--;
                if ((token.Value == "," || token.Value == ";") && bracketsDepth == 0)
                {
                    gatesTokens.Remove(gatesTokens.Last());

                    var instance = new Instance(gatesTokens, this);
                    Gates.Add(instance);
                    gatesTokens = new List<Token>();
                }
            }
        }

        private void ProcessOutput(List<Token> tokens)
        {
            Pin pin = new Pin();
            bool nested = false;
            bool negation = false;

            List<Token> nameTokens = new List<Token>();

            for (int i = 0; i < tokens.Count; i++)
            {

                if (tokens[i].Value == "!")
                {
                    negation = true;
                    continue;
                }

                if (tokens[i].Value == ".")
                {
                    nested = true;
                    continue;
                }

                if (tokens[i].Type == Token.TokenType.Identifier)
                {
                    nameTokens.Add(tokens[i]);
                }

            }

            if (nested)
            {
                var gate = Gates.First(x => x.Name == nameTokens[1].Value);

                pin = gate.Outputs.First(x => x.Name == nameTokens[2].Value);
            }
            else
            {
                pin = Inputs.First(x => x.Name == nameTokens[1].Value);
            }

            if (negation)
            {
                var negationPin1 = new Pin(pin.Name);
                var negationGate = new Negation(negationPin1);

                Links.Add(new Link(
                    negationPin1,
                    pin
                ));

                Gates.Add(negationGate);
                pin = negationGate.Outputs[0];
            }

            Links.Add(new Link(
                Outputs.First(x => nameTokens[0].Value == x.Name),
                pin
            ));

        }
        public override string ToString()
        {
            return $"{Name}\n" +
                   $"inputs = { String.Join(',', Inputs.Select(x => x.Name))}\n" +
                   $"outputs = { String.Join(',', Outputs.Select(x => x.Name))}\n" +
                   $"gates = { String.Join(',', Gates.Select(x => x.Name))}\n" +
                   $"links = { String.Join(',', Links.Select(x => x.Pins[0].Name + "--" + x.Pins[1].Name))}\n";
        }

        private void TakeArguments()
        {
            var bracketsTokens = new List<Token>();

            int bracketsCount = 0;
            foreach (var token in tokens.ToArray())
            {
                bracketsTokens.Add(token);
                tokens.Remove(token);
                switch (token.Value)
                {
                    case ")":
                        ProcessArguments(bracketsTokens, bracketsCount);
                        bracketsCount++;
                        if (bracketsCount > 1)
                        {
                            return;
                        }

                        break;
                    case "(":
                        bracketsTokens = new List<Token>();
                        break;
                }
            }
        }


        private void ProcessArguments(List<Token> tokens, int nr)
        {
            if (nr == 0)
            {
                Inputs = tokens.Where(x => x.Type != Token.TokenType.Separator).Select(y => new Pin(y)).ToList();
            }
            else
            {
                Outputs = tokens.Where(x => x.Type != Token.TokenType.Separator).Select(y => new Pin(y)).ToList();
            }
        }
    }
}
