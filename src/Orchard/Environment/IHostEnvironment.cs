using System;
using System.IO;
using System.Web.Hosting;
using Orchard.Services;

namespace Orchard.Environment {
    /// <summary>
    /// Abstraction of the running environment
    /// </summary>
    public interface IHostEnvironment {
        bool IsFullTrust { get; }
        string MapPath(string virtualPath);

        void RestartAppDomain();
        void ResetSiteCompilation();
    }

    public class DefaultHostEnvironment : IHostEnvironment {
        private readonly IClock _clock;

        public DefaultHostEnvironment(IClock clock) {
            _clock = clock;
        }

        public bool IsFullTrust {
            get { return AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public void RestartAppDomain() {
            ResetSiteCompilation();
        }

        public void ResetSiteCompilation() {
            // Touch web.config
            File.SetLastWriteTimeUtc(MapPath("~/web.config"), _clock.UtcNow);
        }
    }
}
