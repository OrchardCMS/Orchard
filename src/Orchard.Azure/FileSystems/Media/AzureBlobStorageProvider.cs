using System.IO;
using Microsoft.WindowsAzure;
using Orchard.Environment.Configuration;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.FileSystems.Media {
    public class AzureBlobStorageProvider : AzureFileSystem, IStorageProvider {

        public AzureBlobStorageProvider(ShellSettings shellSettings)
            : this(shellSettings, CloudStorageAccount.FromConfigurationSetting("DataConnectionString")) {
        }

        public AzureBlobStorageProvider(ShellSettings shellSettings, CloudStorageAccount storageAccount) : base("media", shellSettings.Name, false, storageAccount) { }


        public bool TrySaveStream(string path, Stream inputStream) {
            try {
                SaveStream(path, inputStream);
            }
            catch {
                return false;
            }

            return true;
        }

        public void SaveStream(string path, Stream inputStream) {
            // Create the file.
            // The CreateFile method will map the still relative path
            var file = CreateFile(path);

            using(var outputStream = file.OpenWrite()) {
                var buffer = new byte[8192];
                for (;;) {
                    var length = inputStream.Read(buffer, 0, buffer.Length);
                    if (length <= 0)
                        break;
                    outputStream.Write(buffer, 0, length);
                }
            }
        }
    }
}