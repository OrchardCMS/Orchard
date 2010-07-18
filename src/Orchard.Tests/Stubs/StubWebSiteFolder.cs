using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.Caching;
using Orchard.FileSystems.WebSite;

namespace Orchard.Tests.Stubs {
    public class StubWebSiteFolder : IWebSiteFolder {
        public IEnumerable<string> ListDirectories(string path) {
            if (!Directory.Exists(path))
                return Enumerable.Empty<string>();

            return Directory.GetDirectories(path);
        }

        public bool FileExists(string virtualPath) {
            throw new NotImplementedException();
        }

        public string ReadFile(string path) {
            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }

        public void CopyFileTo(string virtualPath, Stream destination) {
            throw new NotImplementedException();
        }

        public IVolatileToken WhenPathChanges(string path) {
            return new Token {IsCurrent = true};
        }

        public class Token : IVolatileToken {
            public bool IsCurrent { get; set; }
        }
    }
}