namespace Orchard.Environment.Configuration {
    public interface IShellSettings {
        string Name { get; set; }
    }

    public class ShellSettings : IShellSettings {
        public string Name { get; set; }
    }
}
