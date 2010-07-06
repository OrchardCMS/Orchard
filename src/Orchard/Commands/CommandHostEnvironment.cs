using System;
using System.Linq;
using System.Web.Hosting;
using Orchard.Environment;

namespace Orchard.Commands {
    public class CommandHostEnvironment : IHostEnvironment {
        public bool IsFullTrust {
            get { return AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public bool IsAssemblyLoaded(string name) {
            return AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == name);
        }

        public void RestartAppDomain() {
            //Don't restart AppDomain in command line environment
        }

        public void ResetSiteCompilation() {
            //Don't restart AppDomain in command line environment
        }
    }
}