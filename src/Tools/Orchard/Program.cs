using System;

namespace Orchard {
    public class Program {
        public static int Main(string[] args) {
            return new OrchardHost(Console.In, Console.Out, args).Run();
        }
    }
}
