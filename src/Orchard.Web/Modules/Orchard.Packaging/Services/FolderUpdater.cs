using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;

namespace Orchard.Packaging.Services {
    public interface IFolderUpdater : IDependency {
        void Backup(DirectoryInfo existingFolder, DirectoryInfo backupfolder);
        void Update(DirectoryInfo destinationFolder, DirectoryInfo newFolder);
    }

    [OrchardFeature("Gallery.Updates")]
    public class FolderUpdater : IFolderUpdater {
        private readonly INotifier _notifier;

        public class FolderContent {
            public DirectoryInfo Folder { get; set; }
            public IEnumerable<string> Files { get; set; }
        }

        public FolderUpdater(INotifier notifier) {
            _notifier = notifier;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Backup(DirectoryInfo existingFolder, DirectoryInfo backupfolder) {
            CopyFolder(GetFolderContent(existingFolder), backupfolder);
        }

        public void Update(DirectoryInfo destinationFolder, DirectoryInfo newFolder) {
            var destinationContent = GetFolderContent(destinationFolder);
            var newContent = GetFolderContent(newFolder);

            Update(destinationContent, newContent);
        }

        private void Update(FolderContent destinationContent, FolderContent newContent) {
            // Copy files from new folder to existing folder
            foreach (var file in newContent.Files) {
                CopyFile(newContent.Folder, file, destinationContent.Folder);
            }

            // Delete files that are in the existing folder but not in the new folder
            foreach (var file in destinationContent.Files.Except(newContent.Files, StringComparer.OrdinalIgnoreCase)) {
                var fileToDelete = new FileInfo(Path.Combine(destinationContent.Folder.FullName, file));
                try {
                    fileToDelete.Delete();
                }
                catch (Exception exception) {
                    for (Exception scan = exception; scan != null; scan = scan.InnerException) {
                        _notifier.Warning(T("Unable to delete file \"{0}\": {1}", fileToDelete.FullName, scan.Message));
                    }
                }
            }
        }

        private void CopyFolder(FolderContent source, DirectoryInfo dest) {
            foreach (var file in source.Files) {
                CopyFile(source.Folder, file, dest);
            }
        }

        private void CopyFile(DirectoryInfo sourceFolder, string fileName, DirectoryInfo destinationFolder) {
            var sourceFile = new FileInfo(Path.Combine(sourceFolder.FullName, fileName));
            var destFile = new FileInfo(Path.Combine(destinationFolder.FullName, fileName));

            // If destination file exist, overwrite only if changed
            if (destFile.Exists) {
                if (sourceFile.Length == destFile.Length) {
                    var source = File.ReadAllBytes(sourceFile.FullName);
                    var dest = File.ReadAllBytes(destFile.FullName);
                    if (source.SequenceEqual(dest)) {
                        //_notifier.Information(T("Skipping file \"{0}\" because it is the same content as the source file", destFile.FullName));
                        return;
                    }
                }
            }

            // Create destination directory
            if (!destFile.Directory.Exists) {
                destFile.Directory.Create();
            }

            File.Copy(sourceFile.FullName, destFile.FullName, true);
        }

        private FolderContent GetFolderContent(DirectoryInfo folder) {
            var files = new List<string>();
            GetFolderContent(folder, "", files);
            return new FolderContent { Folder = folder, Files = files };
        }

        private void GetFolderContent(DirectoryInfo folder, string prefix, List<string> files) {
            if (!folder.Exists)
                return;

            foreach (var file in folder.GetFiles()) {
                files.Add(Path.Combine(prefix, file.Name));
            }

            foreach (var child in folder.GetDirectories()) {
                GetFolderContent(child, Path.Combine(prefix, child.Name), files);
            }
        }
    }
}