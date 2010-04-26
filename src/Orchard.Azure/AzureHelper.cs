using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Orchard.Azure {
    public class AzureHelper {

        public static bool BlobExists(CloudBlobContainer container, string path) {
            if ( String.IsNullOrEmpty(path) || path.Trim() == String.Empty )
                throw new ArgumentException("Path can't be empty");

            try {
                var blob = container.GetBlockBlobReference(path);
                blob.FetchAttributes();
                return true;
            }
            catch ( StorageClientException e ) {
                if ( e.ErrorCode == StorageErrorCode.ResourceNotFound ) {
                    return false;
                }

                throw;
            }
        }

        public static void EnsurePathIsRelative(string path) {
            if ( path.StartsWith("/") )
                throw new ArgumentException("Path must be relative");
        }

        public static void EnsureBlobExists(CloudBlobContainer container, string path) {
            if ( !BlobExists(container, path) ) {
                throw new ArgumentException("File " + path + " does not exist");
            }
        }

        public static void EnsureBlobDoesNotExist(CloudBlobContainer container, string path) {
            if ( BlobExists(container, path) ) {
                throw new ArgumentException("File " + path + " already exists");
            }
        }

        public static bool DirectoryExists(CloudBlobContainer container, string path) {
            if ( String.IsNullOrEmpty(path) || path.Trim() == String.Empty )
                throw new ArgumentException("Path can't be empty");

            return container.GetDirectoryReference(path).ListBlobs().Count() > 0;
        }

        public static void EnsureDirectoryExists(CloudBlobContainer container, string path) {
            if ( !DirectoryExists(container, path) ) {
                throw new ArgumentException("Directory " + path + " does not exist");
            }
        }

        public static void EnsureDirectoryDoesNotExist(CloudBlobContainer container, string path) {
            if ( DirectoryExists(container, path) ) {
                throw new ArgumentException("Directory " + path + " already exists");
            }
        }

        public static string Combine(string path1, string path2) {
            EnsurePathIsRelative(path1);
            EnsurePathIsRelative(path2);

            if ( path1 == null || path2 == null )
                throw new ArgumentException("One or more path is null");

            if ( path1.Trim() == String.Empty )
                return path2;

            if ( path2.Trim() == String.Empty )
                return path1;

            var uri1 = new Uri(path1);
            var uri2 = new Uri(path2);

            return uri2.IsAbsoluteUri ? uri2.ToString() : new Uri(uri1, uri2).ToString();
        }
    }
}
