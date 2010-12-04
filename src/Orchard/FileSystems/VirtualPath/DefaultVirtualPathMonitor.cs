using System;
using System.Collections.Generic;
using System.Web.Caching;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Services;

namespace Orchard.FileSystems.VirtualPath {
    public class DefaultVirtualPathMonitor : IVirtualPathMonitor {
        private readonly Thunk _thunk;
        private readonly string _prefix = Guid.NewGuid().ToString("n");
        private readonly IDictionary<string, Weak<Token>> _tokens = new Dictionary<string, Weak<Token>>();
        private readonly IClock _clock;
        private readonly IVirtualPathProvider _virtualPathProvider;

        public DefaultVirtualPathMonitor(IClock clock, IVirtualPathProvider virtualPathProvider) {
            _clock = clock;
            _virtualPathProvider = virtualPathProvider;
            _thunk = new Thunk(this);
        }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            // Fix this to monitor first existing parent directory.
            IVolatileToken token = new Token(virtualPath);
            if (_virtualPathProvider.DirectoryExists(virtualPath)) {
                token = BindToken(virtualPath);
                BindSignal(virtualPath);
            }
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
            BindSignal(virtualPath, _thunk.Signal);

        }

        private void BindSignal(string virtualPath, CacheItemRemovedCallback callback) {
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
                callback);
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

            public override string ToString() {
                return string.Format("IsCurrent: {0}, VirtualPath: \"{1}\"", IsCurrent, VirtualPath);
            }
        }

        class Thunk {
            private readonly Weak<DefaultVirtualPathMonitor> _weak;

            public Thunk(DefaultVirtualPathMonitor provider) {
                _weak = new Weak<DefaultVirtualPathMonitor>(provider);
            }

            public void Signal(string key, object value, CacheItemRemovedReason reason) {
                var provider = _weak.Target;
                if (provider != null)
                    provider.Signal(key, value, reason);
            }
        }
    }
}