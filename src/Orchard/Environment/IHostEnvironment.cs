using System;
using System.Web.Hosting;

namespace Orchard.Environment {
    /// <summary>
    /// Abstraction of the running environment
    /// </summary>
    public interface IHostEnvironment : IDependency {
        bool IsFullTrust { get; }
        string MapPath(string virtualPath);
    }

    public class DefaultHostEnvironment : IHostEnvironment {
        public bool IsFullTrust {
            get { return AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }
    }
}
