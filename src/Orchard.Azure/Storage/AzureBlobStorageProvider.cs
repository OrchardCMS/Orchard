using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Orchard.Storage;

namespace Orchard.Azure.Storage
{
    public interface IDependency {}

    public class AzureBlobStorageProvider : IStorageProvider
    {
        private readonly CloudStorageAccount _storageAccount;
        public CloudBlobClient BlobClient { get; private set; }
        public CloudBlobContainer Container { get; private set; }

        public AzureBlobStorageProvider(string containerName) : this(containerName, CloudStorageAccount.FromConfigurationSetting("DataConnectionString"))
        {
        }

        public AzureBlobStorageProvider(string containerName, CloudStorageAccount storageAccount)
        {
            // Setup the connection to custom storage accountm, e.g. Development Storage
            _storageAccount = storageAccount;

            BlobClient = _storageAccount.CreateCloudBlobClient();
            // Get and create the container if it does not exist
            // The container is named with DNS naming restrictions (i.e. all lower case)
            Container = BlobClient.GetContainerReference(containerName);

            Container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
        }

        private static void EnsurePathIsRelative(string path) {
            if(path.StartsWith("/"))
                throw new ArgumentException("Path must be relative");
        }

        private string GetPrefix(string path) {
            var prefix = String.Concat(Container.Name, "/", path);
            if (prefix.EndsWith("/"))
                return prefix;

            return String.Concat(prefix, "/");
        }

        private bool BlobExists(string path) {
            if(String.IsNullOrEmpty(path) || path.Trim() == String.Empty)
                throw new ArgumentException("Path can't be empty");

            try {
                var blob = Container.GetBlockBlobReference(path);
                blob.FetchAttributes();
                return true;
            }
            catch (StorageClientException e) {
                if (e.ErrorCode == StorageErrorCode.ResourceNotFound) {
                    return false;
                }

                throw;
            }
        }

        private void EnsureBlobExists(string path)
        {
            if (!BlobExists(path)) {
                throw new ArgumentException("File " + path + " does not exist");
            }
        }

        private void EnsureBlobDoesNotExist(string path)
        {
            if (BlobExists(path)) {
                throw new ArgumentException("File " + path + " already exists");
            }
        }

        private bool DirectoryExists(string path)
        {
            if (String.IsNullOrEmpty(path) || path.Trim() == String.Empty)
                throw new ArgumentException("Path can't be empty");

            return Container.GetDirectoryReference(path).ListBlobs().Count() > 0;
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!DirectoryExists(path)) {
                throw new ArgumentException("Directory " + path + " does not exist");
            }
        }

        private void EnsureDirectoryDoesNotExist(string path)
        {
            if (DirectoryExists(path)) {
                throw new ArgumentException("Directory " + path + " already exists");
            }
        }

        #region IStorageProvider Members

        public IStorageFile GetFile(string path) {
            EnsurePathIsRelative(path);
            EnsureBlobExists(path);
            return new AzureBlobFileStorage(Container.GetBlockBlobReference(path));
        }

        public IEnumerable<IStorageFile> ListFiles(string path)
        {
            EnsurePathIsRelative(path);
            foreach (var blobItem in BlobClient.ListBlobsWithPrefix(GetPrefix(path)).OfType<CloudBlockBlob>()) {
                yield return new AzureBlobFileStorage(blobItem);
            }
        }

        public IEnumerable<IStorageFolder> ListFolders(string path)
        {
            EnsurePathIsRelative(path);
            if (!DirectoryExists(path))
            {
                try {
                    CreateFolder(path);
                }
                catch (Exception ex) {
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
            EnsureDirectoryDoesNotExist(path);
            Container.GetDirectoryReference(path);
        }

        public void DeleteFolder(string path) {
            EnsureDirectoryExists(path);
            foreach (var blob in Container.GetDirectoryReference(path).ListBlobs()) {
                if (blob is CloudBlob)
                    ((CloudBlob)blob).Delete();

                if (blob is CloudBlobDirectory)
                    DeleteFolder(blob.Uri.ToString());
            }
        }

        public void RenameFolder(string path, string newPath) {
            EnsurePathIsRelative(path);
            EnsurePathIsRelative(newPath);

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
            EnsurePathIsRelative(path);
            EnsureBlobExists(path);
            var blob = Container.GetBlockBlobReference(path);
            blob.Delete();
        }

        public void RenameFile(string path, string newPath) {
            EnsurePathIsRelative(path);
            EnsurePathIsRelative(newPath);
            EnsureBlobExists(path);
            EnsureBlobDoesNotExist(newPath);

            var blob = Container.GetBlockBlobReference(path);
            var newBlob = Container.GetBlockBlobReference(newPath);
            newBlob.CopyFromBlob(blob);
            blob.Delete();
        }

        public IStorageFile CreateFile(string path) {
            EnsurePathIsRelative(path);
            if (BlobExists(path)) {
                throw new ArgumentException("File " + path + " already exists");
            }

            var blob = Container.GetBlockBlobReference(path);
            blob.OpenWrite().Dispose(); // force file creation
            return new AzureBlobFileStorage(blob);
        }

        public string Combine(string path1, string path2) {
            EnsurePathIsRelative(path1);
            EnsurePathIsRelative(path2);

            if (path1 == null || path2 == null)
                throw new ArgumentException("One or more path is null");

            if (path1.Trim() == String.Empty)
                return path2;

            if (path2.Trim() == String.Empty)
                return path1;

            var uri1 = new Uri(path1);
            var uri2 = new Uri(path2);

            return uri2.IsAbsoluteUri ? uri2.ToString() : new Uri(uri1, uri2).ToString();
        }

        #endregion

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

            #region IStorageFolder Members

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
                if (_blob.Parent != null) {
                    return new AzureBlobFolderStorage(_blob.Parent);
                }
                throw new ArgumentException("Directory " + _blob.Uri + " does not have a parent directory");
            }

            private static long GetDirectorySize(CloudBlobDirectory directoryBlob) {
                long size = 0;

                foreach (var blobItem in directoryBlob.ListBlobs()) {
                    if (blobItem is CloudBlob)
                        size += ((CloudBlob)blobItem).Properties.Length;

                    if (blobItem is CloudBlobDirectory)
                        size += GetDirectorySize((CloudBlobDirectory)blobItem);
                }

                return size;
            }

            #endregion
        }
    }
}
