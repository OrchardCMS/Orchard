using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Tests.Stubs {
    public class InMemoryWebSiteFolder : IWebSiteFolder {
        Dictionary<string, string> _contents = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public void AddFile(string virtualPath, string contents) {
            _contents[Canonical(virtualPath)] = contents;
        }

        private string Canonical(string virtualPath) {
            return virtualPath.Replace("\\", "/");
        }

        public IEnumerable<string> ListDirectories(string virtualPath) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ListFiles(string virtualPath, bool recursive) {
            throw new NotImplementedException();
        }

        public bool FileExists(string virtualPath) {
            return _contents.ContainsKey(virtualPath);
        }

        public string ReadFile(string virtualPath) {
            string value;
            return _contents.TryGetValue(Canonical(virtualPath), out value) ? value : null;
        }

        public string ReadFile(string virtualPath, bool actualContent) {
            throw new NotImplementedException();
        }

        public void CopyFileTo(string virtualPath, Stream destination) {
            string value;
            if (_contents.TryGetValue(Canonical(virtualPath), out value)) {
                var bytes = Encoding.Default.GetBytes(value);
                destination.Write(bytes, 0, bytes.Length);
            }
        }

        public void CopyFileTo(string virtualPath, Stream destination, bool actualContent) {
            throw new NotImplementedException();
        }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            throw new NotImplementedException();
        }
    }
}