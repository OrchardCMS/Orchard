using System;
using System.IO;

namespace Orchard.FileSystems.Media {
    public static class StorageProviderExtensions {
        public static void WriteAllText(this IStorageProvider storageProvider, string path, string contents) {
            if (storageProvider.FileExists(path)) {
                storageProvider.DeleteFile(path);
            }

            var file = storageProvider.CreateFile(path);
            using (var stream = file.OpenWrite())
            using (var streamWriter = new StreamWriter(stream)) {
                streamWriter.Write(contents);
            }
        }

        public static string ReadAllText(this IStorageProvider storageProvider, string path) {
            if (!storageProvider.FileExists(path)) {
                return String.Empty;
            }

            var file = storageProvider.GetFile(path);
            using (var stream = file.OpenRead())
            using (var streamReader = new StreamReader(stream)) {
                return streamReader.ReadToEnd();
            }
        }
    }
}
