﻿using System.IO;
using System.Web;
using Orchard.Azure.Services.Environment.Configuration;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.Services.FileSystems.Media {

    [OrchardFeature(Constants.MediaStorageFeatureName)]
    [OrchardSuppressDependency("Orchard.FileSystems.Media.FileSystemStorageProvider")]
    public class AzureBlobStorageProvider : AzureFileSystem, IStorageProvider {

        public AzureBlobStorageProvider(ShellSettings shellSettings, IMimeTypeProvider mimeTypeProvider, IPlatformConfigurationAccessor pca)
            : this(pca.GetSetting(Constants.MediaStorageStorageConnectionStringSettingName, shellSettings.Name, null),
                   Constants.MediaStorageContainerName,
                   pca.GetSetting(Constants.MediaStorageRootFolderPathSettingName, shellSettings.Name, null) ?? shellSettings.Name,
                   mimeTypeProvider,
                   pca.GetSetting(Constants.MediaStoragePublicHostName, shellSettings.Name, null))
        {
        }

        public AzureBlobStorageProvider(string storageConnectionString, string containerName, string rootFolderPath, IMimeTypeProvider mimeTypeProvider, string publicHostName)
            : base(storageConnectionString, containerName, rootFolderPath, false, mimeTypeProvider, publicHostName) {
        }

        public bool TrySaveStream(string path, Stream inputStream) {
            try {
                if (FileExists(path)) {
                    return false;
                }

                SaveStream(path, inputStream);
            }
            catch {
                return false;
            }

            return true;
        }

        public void SaveStream(string path, Stream inputStream) {
            // Create the file. The CreateFile() method will map the still relative path.
            var file = CreateFile(path);

            using (var outputStream = file.OpenWrite()) {
                var buffer = new byte[8192];
                while (true) {
                    var length = inputStream.Read(buffer, 0, buffer.Length);
                    if (length <= 0)
                        break;
                    outputStream.Write(buffer, 0, length);
                }
            }
        }

        /// <summary>
        /// Retrieves the local path for a given URL within the storage provider.
        /// </summary>
        /// <param name="url">The public URL of the media.</param>
        /// <returns>The corresponding local path.</returns>
        public string GetStoragePath(string url) {
            if (url.StartsWith(_absoluteRoot)) {
                return HttpUtility.UrlDecode(url.Substring(Combine(_absoluteRoot, "/").Length));
            }

            return null;
        }

        public string GetRelativePath(string path) {
            return GetPublicUrl(path);
        }
    }
}