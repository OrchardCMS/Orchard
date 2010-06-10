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

        public string ReadFile(string path) {
            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }

        public IVolatileToken WhenPathChanges(string path) {
            return new WebSiteFolder.Token(path);
        }

        public void WhenPathChanges(string path, Action action) {
            throw new NotImplementedException();
        }
    }
}