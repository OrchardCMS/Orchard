using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Orchard.FileSystems.Media;

namespace Orchard.Azure.Services.FileSystems {

    public class AzureFileSystem {

        public const string FolderEntry = "$$$ORCHARD$$$.$$$";

        private readonly bool _isPrivate;
        private readonly IMimeTypeProvider _mimeTypeProvider;

        protected string _root;
        protected string _absoluteRoot;
        protected string _publicHostName;

        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _container;

        public string StorageConnectionString {
            get;
            protected set;
        }

        public string ContainerName {
            get;
            protected set;
        }

        public CloudStorageAccount StorageAccount {
            get {
                EnsureInitialized();
                return _storageAccount;
            }
        }

        public CloudBlobClient BlobClient {
            get {
                EnsureInitialized();
                return _blobClient;
            }
        }

        public CloudBlobContainer Container {
            get {
                EnsureInitialized();
                return _container;
            }
        }

        public AzureFileSystem(string storageConnectionString, string containerName, string root, bool isPrivate, IMimeTypeProvider mimeTypeProvider, string publicHostName = null) {
            _isPrivate = isPrivate;
            _mimeTypeProvider = mimeTypeProvider;
            StorageConnectionString = storageConnectionString;
            ContainerName = containerName;
            _root = String.IsNullOrEmpty(root) ? "" : root + "/";
            _publicHostName = publicHostName;
        }

        protected void EnsureInitialized() {
            if (_storageAccount != null) {
                return;
            }

            _storageAccount = CloudStorageAccount.Parse(StorageConnectionString);
            _absoluteRoot = Combine(Combine(_storageAccount.BlobEndpoint.AbsoluteUri, ContainerName), _root);

            _blobClient = _storageAccount.CreateCloudBlobClient();
            // Get and create the container if it does not exist
            // The container is named with DNS naming restrictions (i.e. all lower case)
            _container = _blobClient.GetContainerReference(ContainerName);

            _container.CreateIfNotExists(_isPrivate ? BlobContainerPublicAccessType.Off : BlobContainerPublicAccessType.Blob);
        }

        private static string ConvertToRelativeUriPath(string path) {
            var newPath = path.Replace(@"\", "/");

            if (newPath.StartsWith("/", StringComparison.OrdinalIgnoreCase) || newPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || newPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                throw new ArgumentException("Path must be relative");
            }

            return newPath;
        }

        public string Combine(string path1, string path2) {
            if (path1 == null) {
                throw new ArgumentNullException("path1");
            }

            if (path2 == null) {
                throw new ArgumentNullException("path2");
            }

            if (String.IsNullOrEmpty(path2)) {
                return path1;
            }

            if (String.IsNullOrEmpty(path1)) {
                return path2;
            }

            if (path2.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || path2.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                return path2;
            }

            return (path1.TrimEnd('/') + '/' + path2.TrimStart('/'));
        }

        public IStorageFile GetFile(string path) {

            path = ConvertToRelativeUriPath(path);

            Container.EnsureBlobExists(String.Concat(_root, path));
            return new AzureBlobFileStorage(Container.GetBlockBlobReference(String.Concat(_root, path)), _absoluteRoot);
        }

        public bool FileExists(string path) {
            return Container.BlobExists(String.Concat(_root, path));
        }

        public bool FolderExists(string path) {
            return Container.DirectoryExists(String.Concat(_root, path));
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {

            path = path ?? String.Empty;
            path = ConvertToRelativeUriPath(path);

            string prefix = Combine(Combine(Container.Name, _root), path);

            if (!prefix.EndsWith("/")) {
                prefix += "/";
            }

            return BlobClient.ListBlobs(prefix)
                        .OfType<CloudBlockBlob>()
                        .Where(blobItem => !blobItem.Uri.AbsoluteUri.EndsWith(FolderEntry))
                        .Select(blobItem => new AzureBlobFileStorage(blobItem, _absoluteRoot))
                        .ToArray();
        }

        public IEnumerable<IStorageFolder> ListFolders(string path) {

            path = path ?? String.Empty;
            path = ConvertToRelativeUriPath(path);

            // return root folders
            if (String.Concat(_root, path) == String.Empty) {
                return Container.ListBlobs()
                    .OfType<CloudBlobDirectory>()
                    .Select<CloudBlobDirectory, IStorageFolder>(d => new AzureBlobFolderStorage(d, _absoluteRoot))
                    .ToList();
            }

            if (!Container.DirectoryExists(String.Concat(_root, path))) {
                try {
                    CreateFolder(path);
                }
                catch (Exception ex) {
                    throw new ArgumentException(string.Format("The folder could not be created at path: {0}. {1}",
                                                                path, ex));
                }
            }

            return Container.GetDirectoryReference(String.Concat(_root, path))
                .ListBlobs()
                .OfType<CloudBlobDirectory>()
                .Select<CloudBlobDirectory, IStorageFolder>(d => new AzureBlobFolderStorage(d, _absoluteRoot))
                .ToList();
        }

        public bool TryCreateFolder(string path) {
            try {
                if (!Container.DirectoryExists(String.Concat(_root, path))) {
                    CreateFolder(path);
                    return true;
                }

                // return false to be consistent with FileSystemProvider's implementation
                return false;
            }
            catch {
                return false;
            }
        }

        public void CreateFolder(string path) {
            path = ConvertToRelativeUriPath(path);
            Container.EnsureDirectoryDoesNotExist(String.Concat(_root, path));

            // Creating a virtually hidden file to make the directory an existing concept
            CreateFile(Combine(path, FolderEntry));

            int lastIndex;
            while ((lastIndex = path.LastIndexOf('/')) > 0) {
                path = path.Substring(0, lastIndex);
                if (!Container.DirectoryExists(String.Concat(_root, path))) {
                    CreateFile(Combine(path, FolderEntry));
                }
            }
        }

        public void DeleteFolder(string path) {
            path = ConvertToRelativeUriPath(path);

            Container.EnsureDirectoryExists(String.Concat(_root, path));
            foreach (var blob in Container.GetDirectoryReference(String.Concat(_root, path)).ListBlobs()) {
                if (blob is CloudBlockBlob)
                    ((CloudBlockBlob)blob).Delete();

                if (blob is CloudBlobDirectory)
                    DeleteFolder(blob.Uri.ToString().Substring(Container.Uri.ToString().Length + 1 + _root.Length));
            }
        }

        public void RenameFolder(string path, string newPath) {
            path = ConvertToRelativeUriPath(path);
            newPath = ConvertToRelativeUriPath(newPath);

            if (!path.EndsWith("/"))
                path += "/";

            if (!newPath.EndsWith("/"))
                newPath += "/";
            foreach (var blob in Container.GetDirectoryReference(_root + path).ListBlobs()) {
                if (blob is CloudBlockBlob) {
                    string filename = Path.GetFileName(blob.Uri.ToString());
                    string source = String.Concat(path, filename);
                    string destination = String.Concat(newPath, filename);
                    RenameFile(source, destination);
                }

                if (blob is CloudBlobDirectory) {
                    var blobDir = (CloudBlobDirectory)blob;
                    string foldername = blobDir.Prefix.Substring(blobDir.Parent.Prefix.Length);
                    string source = String.Concat(path, foldername);
                    string destination = String.Concat(newPath, foldername);
                    RenameFolder(source, destination);
                }
            }
        }

        public void DeleteFile(string path) {
            path = ConvertToRelativeUriPath(path);

            Container.EnsureBlobExists(Combine(_root, path));
            var blob = Container.GetBlockBlobReference(Combine(_root, path));
            blob.DeleteIfExists();
        }

        public void RenameFile(string path, string newPath) {
            path = ConvertToRelativeUriPath(path);
            newPath = ConvertToRelativeUriPath(newPath);

            Container.EnsureBlobExists(String.Concat(_root, path));
            Container.EnsureBlobDoesNotExist(String.Concat(_root, newPath));

            var blob = Container.GetBlockBlobReference(String.Concat(_root, path));
            var newBlob = Container.GetBlockBlobReference(String.Concat(_root, newPath));
            newBlob.StartCopy(blob);
            blob.Delete();
        }

        public void CopyFile(string path, string newPath) {
            path = ConvertToRelativeUriPath(path);
            newPath = ConvertToRelativeUriPath(newPath);

            Container.EnsureBlobExists(String.Concat(_root, path));
            Container.EnsureBlobDoesNotExist(String.Concat(_root, newPath));

            var blob = Container.GetBlockBlobReference(String.Concat(_root, path));
            var newBlob = Container.GetBlockBlobReference(String.Concat(_root, newPath));
            newBlob.StartCopy(blob);
        }

        public IStorageFile CreateFile(string path) {
            path = ConvertToRelativeUriPath(path);

            if (Container.BlobExists(String.Concat(_root, path))) {
                throw new ArgumentException("File " + path + " already exists");
            }

            // create all folder entries in the hierarchy
            int lastIndex;
            var localPath = path;
            while ((lastIndex = localPath.LastIndexOf('/')) > 0) {
                localPath = localPath.Substring(0, lastIndex);
                var folder = Container.GetBlockBlobReference(String.Concat(_root, Combine(localPath, FolderEntry)));
                folder.OpenWrite().Dispose();
            }

            var blob = Container.GetBlockBlobReference(String.Concat(_root, path));
            var contentType = _mimeTypeProvider.GetMimeType(path);
            if (!String.IsNullOrWhiteSpace(contentType)) {
                blob.Properties.ContentType = contentType;
            }

            blob.UploadFromStream(new MemoryStream(new byte[0]));
            return new AzureBlobFileStorage(blob, _absoluteRoot);
        }

        public string GetPublicUrl(string path) {
            path = ConvertToRelativeUriPath(path);
            var uriBuilder = new UriBuilder(Container.GetBlockBlobReference(String.Concat(_root, path)).Uri);
            if (!string.IsNullOrEmpty(_publicHostName)) uriBuilder.Host = _publicHostName;
            return uriBuilder.Uri.ToString();
        }

        private class AzureBlobFileStorage : IStorageFile {
            private CloudBlockBlob _blob;
            private readonly string _rootPath;

            public AzureBlobFileStorage(CloudBlockBlob blob, string rootPath) {
                _blob = blob;
                _rootPath = rootPath;
            }

            public string GetPath() {
                return _blob.Uri.ToString().Substring(_rootPath.Length).Trim('/');
            }

            public string GetName() {
                return Path.GetFileName(GetPath());
            }

            public long GetSize() {
                return _blob.Properties.Length;
            }

            public DateTime GetLastUpdated() {
                _blob.FetchAttributes();
                return _blob.Properties.LastModified.GetValueOrDefault().DateTime;
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

            public Stream CreateFile() {
                // as opposed to the File System implementation, if nothing is done on the stream
                // the file will be emptied, because Azure doesn't implement FileMode.Truncate
                _blob.DeleteIfExists();
                _blob = _blob.Container.GetBlockBlobReference(_blob.Name);
                _blob.UploadFromStream(new MemoryStream(new byte[0]));
                return OpenWrite();
            }
        }

        private class AzureBlobFolderStorage : IStorageFolder {
            private readonly CloudBlobDirectory _blob;
            private readonly string _rootPath;

            public AzureBlobFolderStorage(CloudBlobDirectory blob, string rootPath) {
                _blob = blob;
                _rootPath = rootPath;
            }

            public string GetName() {
                var path = GetPath();
                return path.Substring(path.LastIndexOf('/') + 1);
            }

            public string GetPath() {
                return _blob.Uri.ToString().Substring(_rootPath.Length).Trim('/');
            }

            public long GetSize() {
                return GetDirectorySize(_blob);
            }

            public DateTime GetLastUpdated() {
                return DateTime.MinValue;
            }

            public IStorageFolder GetParent() {
                if (_blob.Parent != null) {
                    return new AzureBlobFolderStorage(_blob.Parent, _rootPath);
                }
                throw new ArgumentException("Directory " + _blob.Uri + " does not have a parent directory");
            }

            private static long GetDirectorySize(CloudBlobDirectory directoryBlob) {
                long size = 0;

                foreach (var blobItem in directoryBlob.ListBlobs()) {
                    if (blobItem is CloudBlockBlob)
                        size += ((CloudBlockBlob)blobItem).Properties.Length;

                    if (blobItem is CloudBlobDirectory)
                        size += GetDirectorySize((CloudBlobDirectory)blobItem);
                }

                return size;
            }
        }
    }
}
