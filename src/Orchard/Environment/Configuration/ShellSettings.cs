namespace Orchard.Environment.Configuration {
    public interface IShellSettings {
        string Name { get; set; }
        string DataProvider { get; set; }
        string DataFolder { get; set; }
        string DataConnectionString { get; set; }
    }

    public class ShellSettings : IShellSettings {
        public string Name { get; set; }
        public string DataProvider { get; set; }
        public string DataFolder { get; set; }
        public string DataConnectionString { get; set; }
    }
}
