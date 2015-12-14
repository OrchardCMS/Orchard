using Orchard.FileSystems.Media;

namespace Orchard.MediaProcessing.Media {
    public static class StorageProviderExtensions {
        public static void TryDeleteFolder(this IStorageProvider storageProvider, string path) {
            try {
                if (storageProvider.FolderExists(path)) {
                    storageProvider.DeleteFolder(path);
                }
            }
            catch {}
        }

        public static IStorageFile OpenOrCreate(this IStorageProvider storageProvider, string path) {
            if (!storageProvider.FileExists(path)) {
                return storageProvider.CreateFile(path);
            }

            return storageProvider.GetFile(path);
        }
    }
}