using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using Orchard.Caching;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.FileSystems.VirtualPath {

    public class DefaultVirtualPathMonitor : IVirtualPathMonitor {
        private readonly Thunk _thunk;
        private readonly string _prefix = Guid.NewGuid().ToString("n");
        private readonly IDictionary<string, Weak<Token>> _tokens = new Dictionary<string, Weak<Token>>();
        private readonly IClock _clock;

        public DefaultVirtualPathMonitor(IClock clock) {
            _clock = clock;
            _thunk = new Thunk(this);
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IVolatileToken WhenPathChanges(string virtualPath) {
            var token = BindToken(virtualPath);
            try {
                BindSignal(virtualPath);
            }
            catch (HttpException e) {
                // This exception happens if trying to monitor a directory or file
                // inside a directory which doesn't exist
                Logger.Information(e, "Error monitoring file changes on virtual path '{0}'", virtualPath);

                //TODO: Return a token monitoring first existing parent directory.
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
            string key = _prefix + virtualPath;

            //PERF: Don't add in the cache if already present. Creating a "CacheDependency"
            //      object (below) is actually quite expensive.
            if (HostingEnvironment.Cache.Get(key) != null)
                return;

            var cacheDependency = HostingEnvironment.VirtualPathProvider.GetCacheDependency(
                virtualPath,
                new[] { virtualPath },
                _clock.UtcNow);

            Logger.Debug("Monitoring virtual path \"{0}\"", virtualPath);

            HostingEnvironment.Cache.Add(
                key,
                virtualPath,
                cacheDependency,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.NotRemovable,
                callback);
        }

        public void Signal(string key, object value, CacheItemRemovedReason reason) {
            var virtualPath = Convert.ToString(value);
            Logger.Debug("Virtual path changed ({1}) '{0}'", virtualPath, reason.ToString());

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