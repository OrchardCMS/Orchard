using Microsoft.WindowsAzure;
using Orchard.Storage;

namespace Orchard.Azure.Storage {

    public class AzureBlobStorageProvider : AzureFileSystem, IStorageProvider {

        public AzureBlobStorageProvider(string shellName)
            : this(shellName, CloudStorageAccount.FromConfigurationSetting("DataConnectionString")) {
        }

        public AzureBlobStorageProvider(string shellName, CloudStorageAccount storageAccount) : base("media", shellName, false, storageAccount) { }
    }
}


