using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Orchard.Azure.Services.FileSystems {

    public static class CloudBlobContainerExtensions {

        public static bool BlobExists(this CloudBlobContainer container, string path) {
            if (String.IsNullOrEmpty(path) || path.Trim() == String.Empty)
                throw new ArgumentException("Path can't be empty");
            return container.GetBlockBlobReference(path).Exists();
        }

        public static void EnsureBlobExists(this CloudBlobContainer container, string path) {
            if (!BlobExists(container, path)) {
                throw new ArgumentException("File " + path + " does not exist");
            }
        }

        public static void EnsureBlobDoesNotExist(this CloudBlobContainer container, string path) {
            if (BlobExists(container, path)) {
                throw new ArgumentException("File " + path + " already exists");
            }
        }

        public static bool DirectoryExists(this CloudBlobContainer container, string path) {
            if (String.IsNullOrEmpty(path) || path.Trim() == String.Empty) {
                throw new ArgumentException("Path can't be empty");
            }

            return container.GetDirectoryReference(path).ListBlobs().Any();
        }

        public static void EnsureDirectoryExists(this CloudBlobContainer container, string path) {
            if (!DirectoryExists(container, path)) {
                throw new ArgumentException("Directory " + path + " does not exist");
            }
        }

        public static void EnsureDirectoryDoesNotExist(this CloudBlobContainer container, string path) {
            if (DirectoryExists(container, path)) {
                throw new ArgumentException("Directory " + path + " already exists");
            }
        }
    }
}
