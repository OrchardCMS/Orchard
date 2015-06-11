using System;
using System.IO;
using System.Web.Hosting;
using Orchard.Environment.Configuration;

namespace Orchard.Azure.MediaServices.Services.TempFiles {

    /// <summary>
    /// Provides an implementation of temp file management that uses the local file system for storage.
    /// </summary>
    public class LocalTempFileManager : ITempFileManager {
        private readonly Lazy<string> _tempStoragePath;

        public LocalTempFileManager(ShellSettings shellSettings) {
            _tempStoragePath = new Lazy<string>(() => {
                var path = HostingEnvironment.MapPath(String.Format("~/App_Data/AzureMediaTemp/{0}", shellSettings.Name));
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            });
        }

        public string GetPhysicalFilePath(string tempFileName) {
            return Path.Combine(_tempStoragePath.Value, tempFileName);
        }

        public string CreateNewFileName(string extension) {
            string tempFileName, tempFilePath;
            do {
               tempFileName = String.Format("{0}.{1}", Guid.NewGuid(), extension.TrimStart('.'));
               tempFilePath = GetPhysicalFilePath(tempFileName);
            } while (File.Exists(tempFilePath));
            return tempFileName;
        }

        public bool FileExists(string tempFileName) {
            var tempFilePath = GetPhysicalFilePath(tempFileName);
            return File.Exists(tempFilePath);
        }

        public Stream LoadFile(string tempFileName) {
            var tempFilePath = GetPhysicalFilePath(tempFileName);
            return new FileStream(tempFilePath, FileMode.Open);
        }

        public void SaveFile(string tempFileName, Stream stream) {
            var tempFilePath = GetPhysicalFilePath(tempFileName);
            using (var tempFileStream = File.Create(tempFilePath)) {
                stream.CopyTo(tempFileStream);
            }
        }

        public void DeleteFile(string tempFileName) {
            var tempFilePath = GetPhysicalFilePath(tempFileName);
            if (File.Exists(tempFilePath)) {
                File.Delete(tempFilePath);
            }
        }
    }
}