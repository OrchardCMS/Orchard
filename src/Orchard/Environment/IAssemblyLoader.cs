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
        private readonly IEnumerable<IAssemblyNameResolver> _assemblyNameResolvers;
        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        public DefaultAssemblyLoader(IEnumerable<IAssemblyNameResolver> assemblyNameResolvers) {
            _assemblyNameResolvers = assemblyNameResolvers.OrderBy(l => l.Order);
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public Assembly Load(string assemblyName) {
            try {
                return _loadedAssemblies.GetOrAdd(this.ExtractAssemblyShortName(assemblyName), shortName => LoadWorker(shortName, assemblyName));
            }
            catch (Exception e) {
                Logger.Warning(e, "Error loading assembly '{0}'", assemblyName);
                return null;
            }
        }

        private Assembly LoadWorker(string shortName, string fullName) {
            // Try loading the assembly with regular fusion rules first (common case)
            Assembly result = LookupFusion(fullName);
            if (result != null)
                return result;

            // If short assembly name, try to figure out the full assembly name using 
            // a policy compatible with Medium Trust.
            if (shortName == fullName) {
                var resolvedName = _assemblyNameResolvers.Select(r => r.Resolve(shortName)).Where(f => f != null).FirstOrDefault();
                if (resolvedName != null)
                    return Assembly.Load(resolvedName);
            }

            return Assembly.Load(fullName);
        }

        private static Assembly LookupFusion(string fullName) {
            try {
                return Assembly.Load(fullName);
            }
            catch {
                return null;
            }
        }
    }

    public static class AssemblyLoaderExtensions {
        public static string ExtractAssemblyShortName(this IAssemblyLoader assemblyLoader, string fullName) {
            return ExtractAssemblyShortName(fullName);
        }

        public static string ExtractAssemblyShortName(string fullName) {
            int index = fullName.IndexOf(',');
            if (index < 0)
                return fullName;
            return fullName.Substring(0, index);
        }
    }
}
