using System;

namespace OrchardCLI {
    public class Program {
        public static int Main(string[] args) {
            return new CLIHost(Console.In, Console.Out, args).Run();
        }
    }
}
