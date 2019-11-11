using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDL.HDLGenerator;
using HDL.Parser;

namespace HDL.Compiler
{
    class CompilerController
    {
        private Action<CompileResult> onComplite;
        private Action<Token, string> onError;

        private CompileResult result;

        private List<Signal> signals = new List<Signal>();
        private List<Gate> gates = new List<Gate>();

        public CompilerController(List<Token> tokens, Action<CompileResult> onComplite, Action<Token, string> onError = null)
        {
            this.onComplite = onComplite;
            this.onError = onError;

            GenerateBasicGates();
            Start(tokens);
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
            }

            GenerateGates(modules.FirstOrDefault(x => x.Name == "Main"));
        }

        private Module ProcessModule(List<Token> tokens)
        {
            if (tokens[0].Type == Token.TokenType.Identifier)
            {
                return new Module(tokens);
            }
            else
            {
                onError?.Invoke(tokens[0], "Module name not found");
            }

            return null;
        }


        private void GenerateGates(Module mainModule)
        {
            if (mainModule == null)
            {
                Console.WriteLine("Main module not found");
                return;
            }

            result = new CompileResult();

            var inputs = mainModule.Inputs.Select(x => new Signal(x.Name)).ToList();
            var outputs = mainModule.Outputs.Select(x => new Signal(x.Name)).ToList();

            ProcessInstance(
                inputs,
                outputs,
                mainModule,
                "main"
                );

            result.Gates = gates.Where(x => x.Signals.All(y => y.Name != "null")).ToList();
            result.Inputs = inputs;
            result.Outputs = outputs;

            onComplite?.Invoke(result);

        }

        private void ProcessInstance(List<Signal> inputs, List<Signal> outputs, Module module, string nameStack)
        {
            if (module.IsAtomic)
            {
                gates.Add(Gate.Create(module, inputs, outputs.First(), nameStack));
                return;
            }

            List<(List<Signal> inputs, List<Signal> outputs, Instance instance)> instances = new List<(List<Signal> inputs, List<Signal> outputs, Instance instance)>();
            Dictionary<Pin, Signal> mapper = new Dictionary<Pin, Signal>();

            for (int i = 0; i < inputs.Count; i++)
            {
                GetAllConnectedPins(module.Links, module.Inputs[i]).ForEach(x =>
                {
                    mapper[x] = inputs[i];
                });
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                GetAllConnectedPins(module.Links, module.Outputs[i]).ForEach(x =>
                {
                    mapper[x] = outputs[i];
                });
            }


            module.Gates.ForEach(gate =>
            {
                var instance = (gate.Inputs.Select(x => new Signal()).ToList(), gate.Outputs.Select(x => new Signal()).ToList(), gate);
                instances.Add(instance);
            });

            //connect links
            int counter = 0;
            foreach (var link in module.Links)
            {
                var mapped = link.Pins.FirstOrDefault(x => mapper.ContainsKey(x));

                if (mapped == null)
                {
                    var signal = new Signal(nameStack + "_internal_" + counter++);
                    GetAllConnectedPins(module.Links, link.Pins[0]).ForEach(x =>
                    {
                        mapper[x] = signal;
                    });
                    mapped = link.Pins[0];
                }

                foreach (var instance in instances)
                {
                    for (int i = 0; i < instance.instance.Inputs.Count; i++)
                    {
                        if (link.Pins.Any(x => x == instance.instance.Inputs[i]))
                        {
                            instance.inputs[i] = mapper[mapped];
                        }
                    }

                    for (int i = 0; i < instance.instance.Outputs.Count; i++)
                    {
                        if (link.Pins.Any(x => x == instance.instance.Outputs[i]))
                        {
                            instance.outputs[i] = mapper[mapped];
                        }
                    }
                }
            }

            foreach (var instance in instances)
            {
                if (instance.inputs.Any(x => x.Name == "null")) Console.WriteLine("some inputs are not connected");
                if (instance.outputs.Any(x => x.Name == "null")) Console.WriteLine("some outputs are not connected");

                ProcessInstance(instance.inputs, instance.outputs, instance.instance.Module, nameStack + "_" + instance.instance.Name);
            }
        }

        private List<Pin> GetAllConnectedPins(List<Link> links, Pin pin)
        {
            int listSize = 0;
            List<Pin> connectedPins = new List<Pin>() { pin };

            do
            {
                listSize = connectedPins.Count;

                foreach (var link in links)
                {
                    if (link.Pins.Any(x => connectedPins.Contains(x)))
                    {
                        connectedPins.AddRange(link.Pins);
                    }
                }

                connectedPins = connectedPins.Distinct().ToList();
            } while (listSize != connectedPins.Count);

            return connectedPins;

        }

        private void GenerateBasicGates()
        {
            new Module("And")
            {
                Inputs = new List<Pin>() { new Pin("a"), new Pin("b") },
                Outputs = new List<Pin>() { new Pin("y") },
                IsAtomic = true
            };
            new Module("Or")
            {
                Inputs = new List<Pin>() { new Pin("a"), new Pin("b") },
                Outputs = new List<Pin>() { new Pin("y") },
                IsAtomic = true
            };
            new Module("Xor")
            {
                Inputs = new List<Pin>() { new Pin("a"), new Pin("b") },
                Outputs = new List<Pin>() { new Pin("y") },
                IsAtomic = true
            };
            new Module("Not")
            {
                Inputs = new List<Pin>() { new Pin("a") },
                Outputs = new List<Pin>() { new Pin("y") },
                IsAtomic = true
            };
        }
    }
}

