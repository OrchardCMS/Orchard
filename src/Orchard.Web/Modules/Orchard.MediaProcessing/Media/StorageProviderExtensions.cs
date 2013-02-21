using Orchard.FileSystems.Media;

namespace Orchard.MediaProcessing.Media {
    public static class StorageProviderExtensions {
        public static bool FileExists(this IStorageProvider storageProvider, string path) {
            try {
                storageProvider.GetFile(path);
                return true;
            }
            catch {}
            return false;
        }

        public static void TryDeleteFolder(this IStorageProvider storageProvider, string path) {
            try {
                storageProvider.DeleteFolder(path);
            }
            catch {}
        }

        public static IStorageFile OpenOrCreate(this IStorageProvider storageProvider, string path) {
            try {
                return storageProvider.CreateFile(path);
            }
            catch {}
            return storageProvider.GetFile(path);
        }
    }
}