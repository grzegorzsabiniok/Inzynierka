using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HDL.Controll;

namespace HDL.CLI
{
    class CLIManager
    {
        private ControllModule controllModule;
        private string[] runArguments;
        public CLIManager(string[] args)
        {
            runArguments = args;
        }

        public void Init(ControllModule controllModule)
        {
            this.controllModule = controllModule;

            switch (runArguments[0])
            {
                case "create": controllModule.CreateProject(runArguments[1]); break;
                case "compile":
                    {
                        if (runArguments.Length > 1)
                        {
                            Directory.SetCurrentDirectory(Path.Combine(Directory.GetCurrentDirectory(), runArguments[1]));
                        }
                        controllModule.Compile(); break;
                    }

            }
        }
    }
}
