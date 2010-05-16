using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Orchard.FileSystems.Media;

namespace Orchard.Azure {
    public class AzureFileSystem {
        public string ContainerName { get; protected set; }

        private readonly CloudStorageAccount _storageAccount;
        private readonly string _root;
        public CloudBlobClient BlobClient { get; private set; }
        public CloudBlobContainer Container { get; private set; }

        public AzureFileSystem(string containerName, string root, bool isPrivate)
            : this(containerName, root, isPrivate, CloudStorageAccount.FromConfigurationSetting("DataConnectionString")) {
        }

        public AzureFileSystem(string containerName, string root, bool isPrivate, CloudStorageAccount storageAccount) {
            // Setup the connection to custom storage accountm, e.g. Development Storage
            _storageAccount = storageAccount;
            ContainerName = containerName;
            _root = String.IsNullOrEmpty(root) || root == "/" ? String.Empty : root + "/";

            BlobClient = _storageAccount.CreateCloudBlobClient();
            // Get and create the container if it does not exist
            // The container is named with DNS naming restrictions (i.e. all lower case)
            Container = BlobClient.GetContainerReference(ContainerName);
            Container.CreateIfNotExist();

            if ( isPrivate ) {
                Container.SetPermissions(new BlobContainerPermissions
                                         {PublicAccess = BlobContainerPublicAccessType.Off});
            }
            else {
                Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            }
        }

        private static void EnsurePathIsRelative(string path) {
            if ( path.StartsWith("/") || path.StartsWith("http://"))
                throw new ArgumentException("Path must be relative");
        }

        public IStorageFile GetFile(string path) {
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);
            Container.EnsureBlobExists(path);
            return new AzureBlobFileStorage(Container.GetBlockBlobReference(path));
        }

        public bool FileExists(string path) {
            path = String.Concat(_root, path);
            return Container.BlobExists(path);
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {
            EnsurePathIsRelative(path);

            string prefix = String.Concat(Container.Name, "/", _root, path);
            if ( !prefix.EndsWith("/") )
                prefix += "/";

            foreach ( var blobItem in BlobClient.ListBlobsWithPrefix(prefix).OfType<CloudBlockBlob>() ) {
                yield return new AzureBlobFileStorage(blobItem);
            }
        }

        public IEnumerable<IStorageFolder> ListFolders(string path) {
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);

            if ( !Container.DirectoryExists(path) ) {
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
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);

            Container.EnsureDirectoryDoesNotExist(path);
            Container.GetDirectoryReference(path);
        }

        public void DeleteFolder(string path) {
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);

            Container.EnsureDirectoryExists(path);
            foreach ( var blob in Container.GetDirectoryReference(path).ListBlobs() ) {
                if ( blob is CloudBlob )
                    ( (CloudBlob)blob ).Delete();

                if ( blob is CloudBlobDirectory )
                    DeleteFolder(blob.Uri.ToString().Substring(Container.Uri.ToString().Length + 1 + _root.Length));
            }
        }

        public void RenameFolder(string path, string newPath) {
            EnsurePathIsRelative(path);

            EnsurePathIsRelative(newPath);

            if ( !path.EndsWith("/") )
                path += "/";

            if ( !newPath.EndsWith("/") )
                newPath += "/";

            foreach ( var blob in Container.GetDirectoryReference(_root + path).ListBlobs() ) {
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
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);

            Container.EnsureBlobExists(path);
            var blob = Container.GetBlockBlobReference(path);
            blob.Delete();
        }

        public void RenameFile(string path, string newPath) {
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);

            EnsurePathIsRelative(newPath);
            newPath = String.Concat(_root, newPath);

            Container.EnsureBlobExists(path);
            Container.EnsureBlobDoesNotExist(newPath);

            var blob = Container.GetBlockBlobReference(path);
            var newBlob = Container.GetBlockBlobReference(newPath);
            newBlob.CopyFromBlob(blob);
            blob.Delete();
        }

        public IStorageFile CreateFile(string path) {
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);

            if ( Container.BlobExists(path) ) {
                throw new ArgumentException("File " + path + " already exists");
            }

            var blob = Container.GetBlockBlobReference(path);
            blob.OpenWrite().Dispose(); // force file creation
            return new AzureBlobFileStorage(blob);
        }

        public string GetPublicUrl(string path) {
            EnsurePathIsRelative(path);
            path = String.Concat(_root, path);
            Container.EnsureBlobExists(path);
            return Container.GetBlockBlobReference(path).Uri.ToString();
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
                return Path.GetDirectoryName(_blob.Uri.ToString());
            }

            public string GetPath() {
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

    }
}
