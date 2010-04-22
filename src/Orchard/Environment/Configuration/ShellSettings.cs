namespace Orchard.Environment.Configuration {
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This 
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data settings.txt files.
    /// </summary>
    public class ShellSettings {
        public string Name { get; set; }

        public string DataProvider { get; set; }
        public string DataConnectionString { get; set; }
        public string DataTablePrefix { get; set; }

        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }
    }
}
