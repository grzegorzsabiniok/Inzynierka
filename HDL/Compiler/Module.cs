using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private List<Token> tokens;

        public static List<Module> Modules = new List<Module>();
        public Module(List<Token> tokens)
        {
            this.tokens = tokens;
            Name = this.tokens[0].Value;
            this.tokens.RemoveAt(0);
            this.tokens.Remove(this.tokens.Last());
            Console.WriteLine(Modules.Count);
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

                    var instance = new Instance(gatesTokens);
                    Gates.Add(instance);

                    gatesTokens.RemoveRange(0, 4);
                    gatesTokens.Remove(gatesTokens.Last());

                    var splitedByComa = gatesTokens.Aggregate(new List<List<Token>> { new List<Token>() },
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
                        if (argument.Count > 1)
                        {
                            var gate = Gates.First(x => x.Name == argument[0].Value);

                            Links.Add(new Link(instance.Inputs[i], gate.Outputs.First(x => x.Name == argument[2].Value)));
                        }
                        else
                        {
                            Links.Add(new Link(instance.Inputs[i], Inputs.First(x => x.Name == argument[0].Value)));
                        }

                        i++;
                    }

                    gatesTokens = new List<Token>();
                }
            }
        }

        private void ProcessOutput(List<Token> tokens)
        {
            Pin pin = null;
            if (tokens.Count == 4)
            {
                pin = Inputs.First(x => x.Name == tokens[2].Value);
            }
            else
            {
                var gate = Gates.First(x => x.Name == tokens[2].Value);

                pin = gate.Outputs.First(x => x.Name == tokens[4].Value);
            }

            Links.Add(new Link(
                Outputs.First(x => tokens[0].Value == x.Name),
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
