using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HDL.CLI;
using HDL.Compiler;
using HDL.HDLGenerator;
using HDL.Parser;
using File = System.IO.File;

namespace HDL.Controll
{
    class ControllModule
    {
        private static string
            sourceFolder = "source",
            packageFolder = "packages",
            outputFolder = "output",
            outputFileName = "main.vhd",
            configFile = "config.json",
            mainSourceFile = "Main",
            sourceFileExtension = ".mod";

        public ControllModule(CLIManager userInterface)
        {
            userInterface.Init(this);
        }

        public void CreateProject(string name)
        {
            var config = new Config()
            {
                Name = name
            };

            var projectPath = Path.Combine(Directory.GetCurrentDirectory(), name);

            Directory.CreateDirectory(projectPath);
            Directory.CreateDirectory(Path.Combine(projectPath, sourceFolder));
            Directory.CreateDirectory(Path.Combine(projectPath, outputFolder));
            Directory.CreateDirectory(Path.Combine(projectPath, packageFolder));

            using (var file = File.CreateText(Path.Combine(projectPath, configFile)))
            {
                file.Write(JsonSerializer.Serialize(config));
            }

            using (var file = File.CreateText(Path.Combine(projectPath, sourceFolder, mainSourceFile + sourceFileExtension)))
            {
                file.Write("module Main()(){}");
            }
        }

        public void Compile()
        {
            Console.WriteLine("start compile");
            var projectPath = Directory.GetCurrentDirectory();

            //load files
            var files = Directory.GetFiles(Path.Combine(projectPath, sourceFolder));

            var sourceFiles = files
                .Where(y => Path.GetExtension(y) == sourceFileExtension)
                .Select(x => new HDL.Parser.File()
                {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Value = File.ReadAllText(x)
                })
                .ToList();

            Console.WriteLine($"Load {sourceFiles.Count} files");

            new ParserController(sourceFiles, x =>
            {
                Console.WriteLine($"{x.Count} tokens found");
                var compiler = new CompilerController(x, result =>
                {
                    new VHDLGeneratorController().GenerateCode(result, code =>
                    {
                        Console.WriteLine("Result");
                        Console.WriteLine("_______________________");
                        Console.WriteLine(code);
                        File.WriteAllText(Path.Combine(projectPath, outputFolder, outputFileName), code);
                    });
                });
            });
        }
    }
}
