using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using Orchard.FileSystems.Media;

namespace Orchard.Azure {
    public class AzureFileSystem {
        public const string FolderEntry = "$$$ORCHARD$$$.$$$";

        public string ContainerName { get; protected set; }

        private readonly CloudStorageAccount _storageAccount;
        private readonly string _root;
        private readonly string _absoluteRoot;
        public CloudBlobClient BlobClient { get; private set; }
        public CloudBlobContainer Container { get; private set; }

        public AzureFileSystem(string containerName, string root, bool isPrivate)
            : this(containerName, root, isPrivate, CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("DataConnectionString"))) {
        }

        public AzureFileSystem(string containerName, string root, bool isPrivate, CloudStorageAccount storageAccount) {
            // Setup the connection to custom storage accountm, e.g. Development Storage
            _storageAccount = storageAccount;
            ContainerName = containerName;
            _root = String.IsNullOrEmpty(root) ? "": root + "/";
            _absoluteRoot = Combine(Combine(_storageAccount.BlobEndpoint.AbsoluteUri, containerName), root);

            using ( new HttpContextWeaver() ) {

                BlobClient = _storageAccount.CreateCloudBlobClient();
                // Get and create the container if it does not exist
                // The container is named with DNS naming restrictions (i.e. all lower case)
                Container = BlobClient.GetContainerReference(ContainerName);

                Container.CreateIfNotExist();

                Container.SetPermissions(isPrivate
                                             ? new BlobContainerPermissions
                                                   {PublicAccess = BlobContainerPublicAccessType.Off}
                                             : new BlobContainerPermissions
                                                   {PublicAccess = BlobContainerPublicAccessType.Container});
            }

        }

        private static void EnsurePathIsRelative(string path) {
            if ( path.StartsWith("/") || path.StartsWith("http://") || path.StartsWith("https://") )
                throw new ArgumentException("Path must be relative");
        }

        public string Combine(string path1, string path2) {
            if ( path1 == null) {
                throw new ArgumentNullException("path1");
            }

            if ( path2 == null ) {
                throw new ArgumentNullException("path2");
            }

            if ( String.IsNullOrEmpty(path2) ) {
                return path1;
            }

            if ( String.IsNullOrEmpty(path1) ) {
                return path2;
            }

            if ( path2.StartsWith("http://") || path2.StartsWith("https://") ) {
                return path2;
            }

            var ch = path1[path1.Length - 1];

            if (ch != '/') {
                return (path1.TrimEnd('/') + '/' + path2.TrimStart('/'));
            }

            return (path1 + path2);
        }

        public IStorageFile GetFile(string path) {
            EnsurePathIsRelative(path);

            using ( new HttpContextWeaver() ) {
                Container.EnsureBlobExists(String.Concat(_root, path));
                return new AzureBlobFileStorage(Container.GetBlockBlobReference(String.Concat(_root, path)), _absoluteRoot);
            }
        }

        public bool FileExists(string path) {
            using ( new HttpContextWeaver() ) {
                return Container.BlobExists(String.Concat(_root, path));
            }
        }

        public IEnumerable<IStorageFile> ListFiles(string path) {
            path = path ?? String.Empty;
            
            EnsurePathIsRelative(path);

            string prefix = Combine(Combine(Container.Name, _root), path);
            
            if ( !prefix.EndsWith("/") )
                prefix += "/";

            using (new HttpContextWeaver()) {
                return BlobClient
                        .ListBlobsWithPrefix(prefix)
                        .OfType<CloudBlockBlob>()
                        .Where(blobItem => !blobItem.Uri.AbsoluteUri.EndsWith(FolderEntry))
                        .Select(blobItem => new AzureBlobFileStorage(blobItem, _absoluteRoot))
                        .ToArray();
            }
        }

        public IEnumerable<IStorageFolder> ListFolders(string path) {
            path = path ?? String.Empty;

            EnsurePathIsRelative(path);
            using ( new HttpContextWeaver() ) {

                // return root folders
                if (String.Concat(_root, path) == String.Empty) {
                    return Container.ListBlobs()
                        .OfType<CloudBlobDirectory>()
                        .Select<CloudBlobDirectory, IStorageFolder>(d => new AzureBlobFolderStorage(d, _absoluteRoot))
                        .ToList();
                }

                if (!Container.DirectoryExists(String.Concat(_root, path)) ) {
                    try {
                        CreateFolder(path);
                    }
                    catch ( Exception ex ) {
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
            EnsurePathIsRelative(path);
            using (new HttpContextWeaver()) {
                Container.EnsureDirectoryDoesNotExist(String.Concat(_root, path));

                // Creating a virtually hidden file to make the directory an existing concept
                CreateFile(Combine(path, FolderEntry));

                int lastIndex;
                while ((lastIndex = path.LastIndexOf('/')) > 0) {
                    path = path.Substring(0, lastIndex);
                    if(!Container.DirectoryExists(String.Concat(_root, path))) {
                        CreateFile(Combine(path, FolderEntry));
                    }
                }
            }
        }

        public void DeleteFolder(string path) {
            EnsurePathIsRelative(path);

            using ( new HttpContextWeaver() ) {
                Container.EnsureDirectoryExists(String.Concat(_root, path));
                foreach ( var blob in Container.GetDirectoryReference(String.Concat(_root, path)).ListBlobs() ) {
                    if (blob is CloudBlob)
                        ((CloudBlob) blob).Delete();

                    if (blob is CloudBlobDirectory)
                        DeleteFolder(blob.Uri.ToString().Substring(Container.Uri.ToString().Length + 1 + _root.Length));
                }
            }
        }

        public void RenameFolder(string path, string newPath) {
            EnsurePathIsRelative(path);
            EnsurePathIsRelative(newPath);

            if ( !path.EndsWith("/") )
                path += "/";

            if ( !newPath.EndsWith("/") )
                newPath += "/";
            using ( new HttpContextWeaver() ) {
                foreach (var blob in Container.GetDirectoryReference(_root + path).ListBlobs()) {
                    if (blob is CloudBlob) {
                        string filename = Path.GetFileName(blob.Uri.ToString());
                        string source = String.Concat(path, filename);
                        string destination = String.Concat(newPath, filename);
                        RenameFile(source, destination);
                    }

                    if (blob is CloudBlobDirectory) {
                        string foldername = blob.Uri.Segments.Last();
                        string source = String.Concat(path, foldername);
                        string destination = String.Concat(newPath, foldername);
                        RenameFolder(source, destination);
                    }
                }
            }
        }

        public void DeleteFile(string path) {
            EnsurePathIsRelative(path);
            
            using ( new HttpContextWeaver() ) {
                Container.EnsureBlobExists(Combine(_root, path));
                var blob = Container.GetBlockBlobReference(Combine(_root, path));
                blob.Delete();
            }
        }

        public void RenameFile(string path, string newPath) {
            EnsurePathIsRelative(path);
            EnsurePathIsRelative(newPath);

            using ( new HttpContextWeaver() ) {
                Container.EnsureBlobExists(String.Concat(_root, path));
                Container.EnsureBlobDoesNotExist(String.Concat(_root, newPath));

                var blob = Container.GetBlockBlobReference(String.Concat(_root, path));
                var newBlob = Container.GetBlockBlobReference(String.Concat(_root, newPath));
                newBlob.CopyFromBlob(blob);
                blob.Delete();
            }
        }

        public IStorageFile CreateFile(string path) {
            EnsurePathIsRelative(path);
            
            if ( Container.BlobExists(String.Concat(_root, path)) ) {
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
            var contentType = GetContentType(path);
            if (!String.IsNullOrWhiteSpace(contentType)) {
                blob.Properties.ContentType = contentType;
            }

            blob.UploadByteArray(new byte[0]);
            return new AzureBlobFileStorage(blob, _absoluteRoot);
        }

        public string GetPublicUrl(string path) {
            EnsurePathIsRelative(path);
            
            using ( new HttpContextWeaver() ) {
                Container.EnsureBlobExists(String.Concat(_root, path));
                return Container.GetBlockBlobReference(String.Concat(_root, path)).Uri.ToString();
            }
        }

        /// <summary>
        /// Returns the mime-type of the specified file path, looking into IIS configuration and the Registry
        /// </summary>
        private string GetContentType(string path) {
            string extension = Path.GetExtension(path);
            if (String.IsNullOrWhiteSpace(extension)) {
                return "application/unknown";
            }

            try {
                try {
                    string applicationHost = System.Environment.ExpandEnvironmentVariables(@"%windir%\system32\inetsrv\config\applicationHost.config");
                    string webConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(null).FilePath;

                    // search for custom mime types in web.config and applicationhost.config
                    foreach (var configFile in new[] {webConfig, applicationHost}) {
                        if (File.Exists(configFile)) {
                            var xdoc = XDocument.Load(configFile);
                            var mimeMap = xdoc.XPathSelectElements("//staticContent/mimeMap[@fileExtension='" + extension + "']").FirstOrDefault();
                            if (mimeMap != null) {
                                var mimeType = mimeMap.Attribute("mimeType");
                                if (mimeType != null) {
                                    return mimeType.Value;
                                }
                            }
                        }
                    }
                }
                catch {
                    // ignore issues with web.config to fall back to registry
                }

                // search into the registry
                RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(extension.ToLower());
                if (regKey != null) {
                    var contentType = regKey.GetValue("Content Type");
                    if (contentType != null) {
                        return contentType.ToString();
                    }
                }
            }
            catch {
                // if an exception occured return application/unknown
                return "application/unknown";
            }

            return "application/unknown";
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

            public Stream CreateFile() {
                // as opposed to the File System implementation, if nothing is done on the stream
                // the file will be emptied, because Azure doesn't implement FileMode.Truncate
                _blob.DeleteIfExists();
                _blob = _blob.Container.GetBlockBlobReference(_blob.Uri.ToString());
                _blob.OpenWrite().Dispose(); // force file creation

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
                return path.Substring(path.LastIndexOf('/') +1 );
            }

            public string GetPath() {
                using (new HttpContextWeaver()) {
                    return _blob.Uri.ToString().Substring(_rootPath.Length).Trim('/');
                }
            }

            public long GetSize() {
                using (new HttpContextWeaver()) {
                    return GetDirectorySize(_blob);
                }
            }

            public DateTime GetLastUpdated() {
                return DateTime.MinValue;
            }

            public IStorageFolder GetParent() {
                using (new HttpContextWeaver()) {
                    if (_blob.Parent != null) {
                        return new AzureBlobFolderStorage(_blob.Parent, _rootPath);
                    }
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
