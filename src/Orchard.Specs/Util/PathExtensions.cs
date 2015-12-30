using System;
using System.Diagnostics;
using System.IO;
using Path = Bleroy.FluentPath.Path;

namespace Orchard.Specs.Util {
    public static class PathExtensions {
        public static Path GetRelativePath(this Path path, Path basePath) {
            if (path.Equals(basePath))
                return Path.Get(".");

            if (path.Parent.Equals(basePath))
                return path.FileName;

            if (!path.IsDirectory && path.DirectoryName.Equals(basePath.DirectoryName))
                return path.FileName;

            return path.Parent.GetRelativePath(basePath).Combine(path.FileName);
        }


        public static Path DeepCopy(this Path sourcePath, Path targetPath) {
            sourcePath
                .GetFiles("*", true /*recursive*/)
                .ForEach(file => FileCopy(sourcePath, targetPath, file));
            return sourcePath;
        }

        public static Path DeepCopy(this Path sourcePath, string pattern, Path targetPath) {
            sourcePath
                .GetFiles(pattern, true /*recursive*/)
                .ForEach(file => FileCopy(sourcePath, targetPath, file));
            return sourcePath;
        }

        public static Path ShallowCopy(this Path sourcePath, string pattern, Path targetPath) {
            sourcePath
                .GetFiles(pattern, false /*recursive*/)
                .ForEach(file => FileCopy(sourcePath, targetPath, file));
            return sourcePath;
        }

        public static Path ShallowCopy(this Path sourcePath, Predicate<Path> predicatePath, Path targetPath) {
            sourcePath
                .GetFiles(predicatePath, false /*recursive*/)
                .ForEach(file => FileCopy(sourcePath, targetPath, file));
            return sourcePath;
        }

        private static void FileCopy(Path sourcePath, Path targetPath, Path sourceFile) {
            var targetFile = targetPath.Combine(sourceFile.GetRelativePath(sourcePath));
            targetFile.Parent.CreateDirectory();
            // Trace.WriteLine(string.Format("Copying file '{0}' to '{1}'", sourceFile, targetFile));
            File.Copy(sourceFile, targetFile, true /*overwrite*/);
        }
    }
}
