namespace Orchard.Commands {
    public class CommandContext {
        public string Input { get; set; }
        public string Output { get; set; }
        public string Command { get; set; }
        public string[] Arguments { get; set; }
    }
}
