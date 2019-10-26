using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDL.Parser;

namespace HDL.Compiler
{
    class CompilerController
    {
        private Action<GatesStructure> onComplite;
        private Action<Token, string> onError;
        public CompilerController(Action<GatesStructure> onComplite, Action<Token, string> onError = null)
        {
            this.onComplite = onComplite;
            this.onError = onError;

            new Module("And")
            {
                Inputs = new List<Pin>() { new Pin("a"), new Pin("b") },
                Outputs = new List<Pin>() { new Pin("y") }
            };
            new Module("Or")
            {
                Inputs = new List<Pin>() { new Pin("a"), new Pin("b") },
                Outputs = new List<Pin>() { new Pin("y") }
            };
            new Module("Xor")
            {
                Inputs = new List<Pin>() { new Pin("a"), new Pin("b") },
                Outputs = new List<Pin>() { new Pin("y") }
            };
        }

        public void Start(List<Token> tokens)
        {
            var modules = new List<Module>();
            var moduleTokens = new List<Token>();

            foreach (var token in tokens)
            {
                moduleTokens.Add(token);
                switch (token.Value)
                {
                    case "}":
                        modules.Add(ProcessModule(moduleTokens));
                        break;
                    case "module":
                        moduleTokens = new List<Token>();
                        break;
                }
            }

            foreach (var module in modules)
            {
                module.Process();
                Console.WriteLine(module);
            }
        }

        private Module ProcessModule(List<Token> tokens)
        {
            if (tokens[0].Type == Token.TokenType.Identifier)
            {
                return new Module(tokens);
            }
            else
            {
                onError?.Invoke(tokens[0], "Module name not finded");
            }

            return null;
        }
    }
}
