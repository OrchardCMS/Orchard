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
            Themes = new string[0];
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
            Themes = settings.Themes;
        }

        /// <summary>
        /// The name pf the tenant
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The database provider
        /// </summary>
        public string DataProvider { get; set; }

        /// <summary>
        /// The database connection string
        /// </summary>
        public string DataConnectionString { get; set; }

        /// <summary>
        /// The data table prefix added to table names for this tenant
        /// </summary>
        public string DataTablePrefix { get; set; }

        /// <summary>
        /// The host name of the tenant
        /// </summary>
        public string RequestUrlHost { get; set; }
        
        public string RequestUrlPrefix { get; set; }

        /// <summary>
        /// The encryption algorithm used for encryption services
        /// </summary>
        public string EncryptionAlgorithm { get; set; }

        /// <summary>
        /// The encryption key used for encryption services
        /// </summary>
        public string EncryptionKey { get; set; }

        /// <summary>
        /// The hash algorithm used for encryption services
        /// </summary>
        public string HashAlgorithm { get; set; }

        /// <summary>
        /// The hash key used for encryption services
        /// </summary>
        public string HashKey { get; set; }

        /// <summary>
        /// List of available themes for this tenant
        /// </summary>
        public string[] Themes { get; set; }

        /// <summary>
        /// The state is which the tenant is
        /// </summary>
        public TenantState State { get; set; }
    }
}