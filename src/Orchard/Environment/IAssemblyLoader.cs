using System;
using System.Reflection;
using Orchard.Logging;

namespace Orchard.Environment {
    public interface IAssemblyLoader {
        Assembly Load(string assemblyName);
    }

    public class DefaultAssemblyLoader : IAssemblyLoader {
        public DefaultAssemblyLoader() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public Assembly Load(string assemblyName) {
            try {
                return Assembly.Load(assemblyName);
            }
            catch (Exception e) {
                Logger.Warning(e, "Error loading assembly '{0}'", assemblyName);
                return null;
            }
        }
    }
}
