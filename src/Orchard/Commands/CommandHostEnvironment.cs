using System;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using Orchard.Environment;
using Orchard.Localization;

namespace Orchard.Commands {
    internal class CommandHostEnvironment : IHostEnvironment {
        public CommandHostEnvironment() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool IsFullTrust {
            get { return AppDomain.CurrentDomain.IsFullyTrusted; }
        }

        public string MapPath(string virtualPath) {
            return HostingEnvironment.MapPath(virtualPath);
        }

        public bool IsAssemblyLoaded(string name) {
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly => new AssemblyName(assembly.FullName).Name == name);
        }

        public void RestartAppDomain() {
            ResetSiteCompilation();
        }

        public void ResetSiteCompilation() {
            throw new OrchardCommandHostRetryException(T("A change of configuration requires the session to be restarted."));
        }
    }
}