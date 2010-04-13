namespace Orchard.Environment.Configuration {
    public interface IShellSettings {
        string Name { get; }
        string DataProvider { get; }
        string DataConnectionString { get; }
        string DataPrefix { get; }
    }

    public class ShellSettings : IShellSettings {
        public string Name { get; set; }
        public string DataProvider { get; set; }
        public string DataConnectionString { get; set; }
        public string DataPrefix { get; set; }
    }
}
