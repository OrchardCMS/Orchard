using System.Security.Principal;
using System.Threading;

namespace Orchard.Caching {
    /// <summary>
    /// Keep track of nested caches contexts on a given thread
    /// </summary>
    public class DefaultAcquireContextContext : IAcquireContextContext {
        public IAcquireContext Instance {
            get {
                var principal = Thread.CurrentPrincipal as SurrogatePrincipal;
                if (principal == null)
                    return null;
                return principal.AcquireContext;
            }
            set {
                var surrogatePrincipal = Thread.CurrentPrincipal as SurrogatePrincipal;
                if (value == null) {
                    if (surrogatePrincipal != null) {
                        surrogatePrincipal.AcquireContext = null;
                        Thread.CurrentPrincipal = surrogatePrincipal.ActualPrincipal;
                    }
                }
                else {
                    if (surrogatePrincipal == null) {
                        surrogatePrincipal = new SurrogatePrincipal(Thread.CurrentPrincipal);
                        Thread.CurrentPrincipal = surrogatePrincipal;
                    }
                    surrogatePrincipal.AcquireContext = value;
                }
            }
        }

        public class SurrogatePrincipal : IPrincipal {
            private readonly IPrincipal _principal;

            public SurrogatePrincipal(IPrincipal principal) {
                _principal = principal;
            }

            public bool IsInRole(string role) {
                return _principal.IsInRole(role);
            }

            public IIdentity Identity {
                get { return _principal.Identity; }
            }

            public IPrincipal ActualPrincipal {
                get { return _principal; }
            }

            public IAcquireContext AcquireContext { get; set; }
        }
    }
}