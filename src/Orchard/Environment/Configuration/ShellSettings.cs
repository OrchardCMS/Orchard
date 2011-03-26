namespace Orchard.Environment.Configuration {
    /// <summary>
    /// Represents the minimalistic set of fields stored for each tenant. This 
    /// model is obtained from the IShellSettingsManager, which by default reads this
    /// from the App_Data settings.txt files.
    /// </summary>
    public class ShellSettings {
        public const string DefaultName = "Default";

        public ShellSettings() {
            State = new TenantState("Invalid");
        }

        public ShellSettings(ShellSettings settings) {
            Name = settings.Name;
            DataProvider = settings.DataProvider;
            DataConnectionString = settings.DataConnectionString;
            DataTablePrefix = settings.DataTablePrefix;
            RequestUrlHost = settings.RequestUrlHost;
            RequestUrlPrefix = settings.RequestUrlPrefix;
            EncryptionAlgorithm = settings.EncryptionAlgorithm;
            EncryptionKey = settings.EncryptionKey;
            HashAlgorithm = settings.HashAlgorithm;
            HashKey = settings.HashKey;
            State = settings.State;
        }

        public string Name { get; set; }

        public string DataProvider { get; set; }
        public string DataConnectionString { get; set; }
        public string DataTablePrefix { get; set; }

        public string RequestUrlHost { get; set; }
        public string RequestUrlPrefix { get; set; }

        public string EncryptionAlgorithm { get; set; }
        public string EncryptionKey { get; set; }

        public string HashAlgorithm { get; set; }
        public string HashKey { get; set; }

        public TenantState State { get; set; }
    }
}
