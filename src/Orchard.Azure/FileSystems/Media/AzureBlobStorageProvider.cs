using Microsoft.WindowsAzure;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.FileSystems.Media {
    public class AzureBlobStorageProvider : AzureFileSystem, IStorageProvider {

        public AzureBlobStorageProvider(ShellSettings shellSettings)
            : this(shellSettings, CloudStorageAccount.FromConfigurationSetting("DataConnectionString")) {
        }

        public AzureBlobStorageProvider(ShellSettings shellSettings, CloudStorageAccount storageAccount) : base("media", shellSettings.Name, false, storageAccount) { }


        public bool TrySaveStream(string path, System.IO.Stream inputStream) {
            throw new System.NotImplementedException();
        }

        public void SaveStream(string path, System.IO.Stream inputStream) {
            throw new System.NotImplementedException();
        }
    }
}