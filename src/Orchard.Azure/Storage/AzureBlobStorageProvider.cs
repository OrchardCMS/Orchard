using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Orchard.Storage;

namespace Orchard.Azure.Storage {
    public interface IDependency { }

    public class AzureBlobStorageProvider : IStorageProvider {
        private readonly CloudStorageAccount _storageAccount;
        public CloudBlobClient BlobClient { get; private set; }
        public CloudBlobContainer Container { get; private set; }

        public AzureBlobStorageProvider(string containerName)
            : this(containerName, CloudStorageAccount.FromConfigurationSetting("DataConnectionString")) {
        }

        public AzureBlobStorageProvider(string containerName, CloudStorageAccount storageAccount) {
            // Setup the connection to custom storage accountm, e.g. Development Storage
            _storageAccount = storageAccount;

            BlobClient = _storageAccount.CreateCloudBlobClient();
            // Get and create the container if it does not exist
            // The container is named with DNS naming restrictions (i.e. all lower case)
            Container = BlobClient.GetContainerReference(containerName);
            Container.CreateIfNotExist();

            Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
        }

        public IStorageFile GetFile(string path) {
            AzureHelper.EnsurePathIsRelative(path);
            AzureHelper.EnsureBlobExists(Container, path);
            return new AzureBlobFileStorage(Container.GetBlockBlobReference(path));
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {
            AzureHelper.EnsurePathIsRelative(path);

            string prefix = String.Concat(Container.Name, "/", path);
            if ( !prefix.EndsWith("/") )
                prefix += "/";

            foreach ( var blobItem in BlobClient.ListBlobsWithPrefix(prefix).OfType<CloudBlockBlob>() ) {
                yield return new AzureBlobFileStorage(blobItem);
            }
        }

        public IEnumerable<IStorageFolder> ListFolders(string path) {
            AzureHelper.EnsurePathIsRelative(path);
            if ( !AzureHelper.DirectoryExists(Container, path) ) {
                try {
                    CreateFolder(path);
                }
                catch ( Exception ex ) {
                    throw new ArgumentException(string.Format("The folder could not be created at path: {0}. {1}", path, ex));
                }
            }

            return Container.GetDirectoryReference(path)
                .ListBlobs()
                .OfType<CloudBlobDirectory>()
                .Select<CloudBlobDirectory, IStorageFolder>(d => new AzureBlobFolderStorage(d))
                .ToList();
        }

        public void CreateFolder(string path) {
            AzureHelper.EnsurePathIsRelative(path);
            AzureHelper.EnsureDirectoryDoesNotExist(Container, path);
            Container.GetDirectoryReference(path);
        }

        public void DeleteFolder(string path) {
            AzureHelper.EnsureDirectoryExists(Container, path);
            foreach ( var blob in Container.GetDirectoryReference(path).ListBlobs() ) {
                if ( blob is CloudBlob )
                    ( (CloudBlob)blob ).Delete();

                if ( blob is CloudBlobDirectory )
                    DeleteFolder(blob.Uri.ToString());
            }
        }

        public void RenameFolder(string path, string newPath) {
            AzureHelper.EnsurePathIsRelative(path);
            AzureHelper.EnsurePathIsRelative(newPath);

            if ( !path.EndsWith("/") )
                path += "/";

            if ( !newPath.EndsWith("/") )
                newPath += "/";

            foreach ( var blob in Container.GetDirectoryReference(path).ListBlobs() ) {
                if ( blob is CloudBlob ) {
                    string filename = Path.GetFileName(blob.Uri.ToString());
                    string source = String.Concat(path, filename);
                    string destination = String.Concat(newPath, filename);
                    RenameFile(source, destination);
                }

                if ( blob is CloudBlobDirectory ) {
                    string foldername = blob.Uri.Segments.Last();
                    string source = String.Concat(path, foldername);
                    string destination = String.Concat(newPath, foldername);
                    RenameFolder(source, destination);
                }
            }

        }

        public void DeleteFile(string path) {
            AzureHelper.EnsurePathIsRelative(path);
            AzureHelper.EnsureBlobExists(Container, path);
            var blob = Container.GetBlockBlobReference(path);
            blob.Delete();
        }

        public void RenameFile(string path, string newPath) {
            AzureHelper.EnsurePathIsRelative(path);
            AzureHelper.EnsurePathIsRelative(newPath);
            AzureHelper.EnsureBlobExists(Container, path);
            AzureHelper.EnsureBlobDoesNotExist(Container, newPath);

            var blob = Container.GetBlockBlobReference(path);
            var newBlob = Container.GetBlockBlobReference(newPath);
            newBlob.CopyFromBlob(blob);
            blob.Delete();
        }

        public IStorageFile CreateFile(string path) {
            AzureHelper.EnsurePathIsRelative(path);
            if ( AzureHelper.BlobExists(Container, path) ) {
                throw new ArgumentException("File " + path + " already exists");
            }

            var blob = Container.GetBlockBlobReference(path);
            blob.OpenWrite().Dispose(); // force file creation
            return new AzureBlobFileStorage(blob);
        }

        private class AzureBlobFileStorage : IStorageFile {
            private readonly CloudBlockBlob _blob;

            public AzureBlobFileStorage(CloudBlockBlob blob) {
                _blob = blob;
            }

            public string GetPath() {
                return _blob.Uri.ToString();
            }

            public string GetName() {
                return Path.GetFileName(GetPath());
            }

            public long GetSize() {
                return _blob.Properties.Length;
            }

            public DateTime GetLastUpdated() {
                return _blob.Properties.LastModifiedUtc;
            }

            public string GetFileType() {
                return Path.GetExtension(GetPath());
            }

            public Stream OpenRead() {
                return _blob.OpenRead();
            }

            public Stream OpenWrite() {
                return _blob.OpenWrite();
            }

        }

        private class AzureBlobFolderStorage : IStorageFolder {
            private readonly CloudBlobDirectory _blob;

            public AzureBlobFolderStorage(CloudBlobDirectory blob) {
                _blob = blob;
            }

            public string GetName() {
                return _blob.Uri.ToString();
            }

            public long GetSize() {
                return GetDirectorySize(_blob);
            }

            public DateTime GetLastUpdated() {
                return DateTime.MinValue;
            }

            public IStorageFolder GetParent() {
                if ( _blob.Parent != null ) {
                    return new AzureBlobFolderStorage(_blob.Parent);
                }
                throw new ArgumentException("Directory " + _blob.Uri + " does not have a parent directory");
            }

            private static long GetDirectorySize(CloudBlobDirectory directoryBlob) {
                long size = 0;

                foreach ( var blobItem in directoryBlob.ListBlobs() ) {
                    if ( blobItem is CloudBlob )
                        size += ( (CloudBlob)blobItem ).Properties.Length;

                    if ( blobItem is CloudBlobDirectory )
                        size += GetDirectorySize((CloudBlobDirectory)blobItem);
                }

                return size;
            }
        }

        #region IStorageProvider Members


        public string Combine(string path1, string path2) {
            return AzureHelper.Combine(path1, path2);
        }

        #endregion
    }
}
