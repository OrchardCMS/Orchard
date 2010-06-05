using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Services;

namespace Orchard.FileSystems.WebSite {
    public class WebSiteFolder : IWebSiteFolder {
        private readonly IClock _clock;
        private readonly Thunk _thunk;
        private readonly string _prefix = Guid.NewGuid().ToString("n");
        private readonly IDictionary<string, Weak<Token>> _tokens = new Dictionary<string, Weak<Token>>();


        public WebSiteFolder(IClock clock) {
            _clock = clock;
            _thunk = new Thunk(this);
        }

        public IEnumerable<string> ListDirectories(string virtualPath) {
            if (!HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualPath))
                return Enumerable.Empty<string>();

            return HostingEnvironment.VirtualPathProvider
                .GetDirectory(virtualPath)
                .Directories.OfType<VirtualDirectory>()
                .Select(d => d.VirtualPath)
                .ToArray();
        }

        public string ReadFile(string virtualPath) {
            if (!HostingEnvironment.VirtualPathProvider.FileExists(virtualPath))
                return null;

            using (var stream = VirtualPathProvider.OpenFile(virtualPath)) {
                using (var reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            // Fix this to monitor first existing parent directory.
            var token = BindToken(virtualPath);
            BindSignal(virtualPath);
            return token;
        }

        private Token BindToken(string virtualPath) {
            lock (_tokens) {
                Weak<Token> weak;
                if (!_tokens.TryGetValue(virtualPath, out weak)) {
                    weak = new Weak<Token>(new Token(virtualPath));
                    _tokens[virtualPath] = weak;
                }

                var token = weak.Target;
                if (token == null) {
                    token = new Token(virtualPath);
                    weak.Target = token;
                }

                return token;
            }
        }

        private Token DetachToken(string virtualPath) {
            lock (_tokens) {
                Weak<Token> weak;
                if (!_tokens.TryGetValue(virtualPath, out weak)) {
                    return null;
                }
                var token = weak.Target;
                weak.Target = null;
                return token;
            }
        }

        private void BindSignal(string virtualPath) {
            var cacheDependency = HostingEnvironment.VirtualPathProvider.GetCacheDependency(
                virtualPath,
                new[] { virtualPath },
                _clock.UtcNow);

            HostingEnvironment.Cache.Add(
                _prefix + virtualPath,
                virtualPath,
                cacheDependency,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                _thunk.Signal);
        }

        public void Signal(string key, object value, CacheItemRemovedReason reason) {
            var virtualPath = Convert.ToString(value);
            var token = DetachToken(virtualPath);
            if (token != null)
                token.IsCurrent = false;
        }

        public class Token : IVolatileToken {
            public Token(string virtualPath) {
                IsCurrent = true;
                VirtualPath = virtualPath;
            }
            public bool IsCurrent { get; set; }
            public string VirtualPath { get; private set; }
        }

        class Thunk {
            private readonly Weak<WebSiteFolder> _weak;

            public Thunk(WebSiteFolder provider) {
                _weak = new Weak<WebSiteFolder>(provider);
            }

            public void Signal(string key, object value, CacheItemRemovedReason reason) {
                var provider = _weak.Target;
                if (provider != null)
                    provider.Signal(key, value, reason);
            }
        }
    }
}