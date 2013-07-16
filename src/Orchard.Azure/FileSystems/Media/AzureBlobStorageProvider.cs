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

        /// <summary>
        /// Retrieves the local path for a given url within the storage provider.
        /// </summary>
        /// <param name="url">The public url of the media.</param>
        /// <returns>The local path.</returns>
        public string GetStoragePath(string url) {
            if (url.StartsWith(_absoluteRoot)) {
                return url.Substring(Combine(_absoluteRoot, "/").Length);
            }

            return null;
        }

        public string GetRelativePath(string path) {
            return GetPublicUrl(path);
        }

    }
}