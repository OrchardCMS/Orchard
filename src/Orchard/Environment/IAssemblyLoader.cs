using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Caching;
using Orchard.Logging;

namespace Orchard.Environment {
    public interface IAssemblyLoader {
        Assembly Load(string assemblyName);
    }

    public class DefaultAssemblyLoader : IAssemblyLoader {
        private readonly ICacheManager _cacheManager;
        private readonly ConcurrentDictionary<string, Assembly> _loadedAssemblies = new ConcurrentDictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);

        public DefaultAssemblyLoader(ICacheManager cacheManager) {
            _cacheManager = cacheManager;
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
            // If short assembly name, try to figure out the full assembly name using 
            // a policy compatible with Medium Trust.
            if (shortName == fullName) {
                // Look in assemblies loaded in the AppDomain first.
                var result = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => StringComparer.OrdinalIgnoreCase.Equals(shortName, this.ExtractAssemblyName(a.FullName)))
                    .SingleOrDefault();

                if (result != null)
                    return result;

                // A few common .net framework assemblies are referenced by the Orchard.Framework assembly.
                // Look into those to see if we can find the assembly we are looking for.
                var orchardFrameworkReferences = _cacheManager.Get(
                    typeof(IAssemblyLoader),
                    ctx => ctx.Key.Assembly
                                .GetReferencedAssemblies()
                                .GroupBy(n => this.ExtractAssemblyName(n.FullName), StringComparer.OrdinalIgnoreCase)
                                .ToDictionary(n => n.Key /*short assembly name*/, g => g.OrderBy(n => n.Version).Last() /* highest assembly version */, StringComparer.OrdinalIgnoreCase));

                AssemblyName assemblyName;
                if (orchardFrameworkReferences.TryGetValue(shortName, out assemblyName)) {
                    return Assembly.Load(assemblyName.FullName);
                }
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
