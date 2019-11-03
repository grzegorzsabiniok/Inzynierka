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

        public CompilerController(Action<CompileResult> onComplite, Action<Token, string> onError = null)
        {
            this.onComplite = onComplite;
            this.onError = onError;

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
                onError?.Invoke(tokens[0], "Module name not finded");
            }

            return null;
        }

        private CompileResult result;

        private List<Signal> signals = new List<Signal>();
        private List<Gate> gates = new List<Gate>();
        private void GenerateGates(Module mainModule)
        {
            if (mainModule == null)
            {
                Console.WriteLine("Main module not found");
                return;
            }

            result = new CompileResult();

            //var mainInstance = new Instance(mainModule, "main");

            var inputs = mainModule.Inputs.Select(x => new Signal(x.Name)).ToList();
            var outputs = mainModule.Outputs.Select(x => new Signal(x.Name)).ToList();

            ProcessInstance(
                inputs,
                outputs,
                mainModule,
                "main"
                );

            result.Gates = gates;
            result.Inputs = inputs;
            result.Outputs = outputs;

            onComplite?.Invoke(result);

        }

        private void ProcessInstance(List<Signal> inputs, List<Signal> outputs, Module module, string nameStack)
        {
            if (module.IsAtomic)
            {
                gates.Add(new Gate(module, inputs, outputs.First(), nameStack));
                return;
            }

            List<(List<Signal> inputs, List<Signal> outputs, Instance instance)> instances = new List<(List<Signal> inputs, List<Signal> outputs, Instance instance)>();
            Dictionary<Pin, Signal> mapper = new Dictionary<Pin, Signal>();

            for (int i = 0; i < inputs.Count; i++)
            {
                mapper.Add(module.Inputs[i], inputs[i]);
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                mapper.Add(module.Outputs[i], outputs[i]);
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
                    link.Pins.ForEach(pin => mapper.Add(pin, signal));
                    mapped = link.Pins.First();
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
                if (instance.inputs.Any(x => x.Name == "null")) Console.WriteLine("fuck");
                if (instance.outputs.Any(x => x.Name == "null")) Console.WriteLine("fuck2");

                ProcessInstance(instance.inputs, instance.outputs, instance.instance.Module, nameStack + "_" + instance.instance.Name);
            }
        }
    }
}

//    ProcessInstance(mainInstance, "main");

//    links.ForEach(x => Console.WriteLine(String.Join(",", x.Pins.Select(z => z.Name))));

//    foreach (var instance in instances)
//    {
//        for (int i = 0; i < instance.Inputs.Count; i++)
//        {
//            var linkWithPin = links.FirstOrDefault(x => x.Pins.Any(y => y == instance.Inputs[i]));

//            if (linkWithPin != null)
//            {
//                instance.Inputs[i] = linkWithPin.Pins.First(x => x != instance.Inputs[i]);
//                Console.WriteLine("znaleziono");
//                links.Remove(linkWithPin);

//            }
//            else
//            {
//                Console.WriteLine("nie znaleziono" + instance.Name + "|" + instance.Inputs[i].Name);
//            }

//        }
//    }

//    Console.WriteLine("links:" + instances.Count);

//    instances.ForEach(x => Console.WriteLine(String.Join(",", x.Inputs.Select(z => z.Name))));

//    for (int i = 0; i < mainInstance.Inputs.Count; i++)
//    {
//        mainInstance.Inputs[i].Name = mainInstance.Module.Inputs[i].Name;
//    }

//    result.Inputs = mainInstance.Inputs;
//    result.Outputs = mainInstance.Outputs;

//    result.Gates = instances.Where(x => x.Module.IsAtomic).Select(y => new Gate(y)).ToList();

//    onComplite?.Invoke(result);
//}

//private List<Signal> signals = new List<Signal>();
//private List<Link> links = new List<Link>();
//private List<Instance> instances = new List<Instance>();

//private void ProcessInstance(Instance instance, string nameChain)
//{
//    instances.Add(instance);

//    var localLinks = instance.Module.Links.Select(x => new Link(x)).ToList();
//    links.AddRange(localLinks);

//    for (int i = 0; i < instance.Inputs.Count; i++)
//    {
//        instance.Inputs[i].Name = "s_" + nameChain + "_" + instance.Inputs[i].Name;
//        ReplacePin(instance.Module.Inputs[i], instance.Inputs[i]);
//    }

//    for (int i = 0; i < instance.Outputs.Count; i++)
//    {
//        instance.Outputs[i].Name = "s_" + nameChain + "_" + instance.Outputs[i].Name;
//        ReplacePin(instance.Module.Outputs[i], instance.Outputs[i]);
//    }

//    foreach (var gate in instance.Module.Gates)
//    {
//        var localGate = new Instance(gate.Module, nameChain + "_" + gate.Name);

//        for (int i = 0; i < localGate.Inputs.Count; i++)
//        {
//            ReplacePin(gate.Module.Inputs[i], localGate.Inputs[i]);
//        }

//        for (int i = 0; i < localGate.Outputs.Count; i++)
//        {
//            ReplacePin(gate.Module.Outputs[i], localGate.Outputs[i]);
//        }

//        ProcessInstance(localGate, localGate.Name);
//    }
//}

//private void ReplacePin(Pin from, Pin to)
//{
//    if (!links.Any(x => x.Pins.Any(y => y == from)))
//    {
//        Console.WriteLine("nie udalo sie znalezc" + from.Name);
//    }

//    links.ForEach(x => x.Pins = x.Pins.Select(y => y == from ? to : y).ToList());
//}
//    }
//}
