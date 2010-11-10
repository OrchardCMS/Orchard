using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Logging;

namespace Orchard.Environment {
    public interface IAssemblyLoader {
        Assembly Load(string assemblyName);
    }

    public class DefaultAssemblyLoader : IAssemblyLoader {
        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        public DefaultAssemblyLoader() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public Assembly Load(string assemblyName) {
            try {
                return _loadedAssemblies.GetOrAdd(this.ExtractAssemblyName(assemblyName), shortName => LoadWorker(shortName, assemblyName));
            }
            catch (Exception e) {
                Logger.Warning(e, "Error loading assembly '{0}'", assemblyName);
                return null;
            }
        }
        
        private Assembly LoadWorker(string shortName, string fullName) {
            // If short assembly name, look in list of loaded assemblies first
            if (shortName == fullName) {
                var result = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => StringComparer.OrdinalIgnoreCase.Equals(shortName, a.GetName().Name))
                    .SingleOrDefault();

                if (result != null)
                    return result;
            }

            return Assembly.Load(fullName);
        }
    }

    public static class AssemblyLoaderExtensions {
        public static string ExtractAssemblyName(this IAssemblyLoader assemblyLoader, string fullName) {
            int index = fullName.IndexOf(',');
            if (index < 0)
                return fullName;
            return fullName.Substring(0, index);
        }
    }
}
