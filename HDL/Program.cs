using System;
using HDL.CLI;
using HDL.Controll;

namespace HDL
{
    class Program
    {
        static void Main(string[] args)
        {
            var cliManager = new CLIManager(args);
            var controllModule = new ControllModule(cliManager);
        }
    }
}
