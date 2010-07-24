namespace OrchardCLI {
    public class Program {
        public static int Main(string[] args) {
            return new CLIHost(args).Run();
        }
    }
}
