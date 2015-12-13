using System;
using System.Collections.Generic;
using System.IO;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    public class InMemoryWebSiteFolder : IWebSiteFolder {
        public InMemoryWebSiteFolder() {
            Contents = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, string> Contents { get; set; }

        public IEnumerable<string> ListDirectories(string virtualPath) {
            throw new NotImplementedException();
        }

        public IEnumerable<string> ListFiles(string virtualPath, bool recursive) {
            throw new NotImplementedException();
        }

        public bool FileExists(string virtualPath) {
            throw new NotImplementedException();
        }

        public string ReadFile(string virtualPath) {
            string result;
            return Contents.TryGetValue(virtualPath, out result) ? result : null;
        }

        public string ReadFile(string virtualPath, bool actualContent) {
            throw new NotImplementedException();
        }

        public void CopyFileTo(string virtualPath, Stream destination) {
            throw new NotImplementedException();
        }

        public void CopyFileTo(string virtualPath, Stream destination, bool actualContent) {
            throw new NotImplementedException();
        }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            return new Token { IsCurrent = true };
        }

        public class Token : IVolatileToken {
            public bool IsCurrent { get; set; }
        }
    }
}