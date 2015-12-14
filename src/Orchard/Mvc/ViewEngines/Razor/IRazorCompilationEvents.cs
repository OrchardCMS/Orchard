using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Razor.Generator;
using System.Web.WebPages.Razor;
using Orchard.Environment;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.Dependencies;
using Orchard.Logging;

namespace Orchard.Mvc.ViewEngines.Razor {
    public interface IRazorCompilationEvents {
        void CodeGenerationStarted(RazorBuildProvider provider);
        void CodeGenerationCompleted(RazorBuildProvider provider, CodeGenerationCompleteEventArgs e);
    }

    /// <summary>
    /// The purpose of this class is to notify the Razor View Engine of Module and Theme
    /// dependencies when compiling Views, so that the Razor Views build provider will add proper 
    /// assembly references when calling the compiler.
    /// For example, if Module A depends on Module Bar and some other 3rd party DLL "Foo.dll",
    /// we will notify the Razor View Engine of the following dependencies:
    /// * BuildManager.GetCompiledAssembly("~/Modules/Bar/Bar.csproj");
    /// * Assembly.Load("Foo");
    /// </summary>
    public class DefaultRazorCompilationEvents : IRazorCompilationEvents {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IExtensionDependenciesManager _extensionDependenciesManager;
        private readonly IBuildManager _buildManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly IAssemblyLoader _assemblyLoader;

        public DefaultRazorCompilationEvents(
            IDependenciesFolder dependenciesFolder,
            IExtensionDependenciesManager extensionDependenciesManager,
            IBuildManager buildManager,
            IEnumerable<IExtensionLoader> loaders,
            IAssemblyLoader assemblyLoader) {

            _dependenciesFolder = dependenciesFolder;
            _extensionDependenciesManager = extensionDependenciesManager;
            _buildManager = buildManager;
            _loaders = loaders;
            _assemblyLoader = assemblyLoader;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void CodeGenerationStarted(RazorBuildProvider provider) {
            var assembliesToAdd = new List<Assembly>();

            DependencyDescriptor moduleDependencyDescriptor = GetModuleDependencyDescriptor(provider.VirtualPath);

            IEnumerable<DependencyDescriptor> dependencyDescriptors = _dependenciesFolder.LoadDescriptors();
            List<DependencyDescriptor> filteredDependencyDescriptors;
            if (moduleDependencyDescriptor != null) {
                // Add module
                filteredDependencyDescriptors = new List<DependencyDescriptor> { moduleDependencyDescriptor };

                // Add module's references
                filteredDependencyDescriptors.AddRange(moduleDependencyDescriptor.References
                    .Select(reference => dependencyDescriptors
                        .FirstOrDefault(dependency => dependency.Name == reference.Name)
                        ?? new DependencyDescriptor {
                            LoaderName = reference.LoaderName,
                            Name = reference.Name,
                            VirtualPath = reference.VirtualPath
                            }
                        ));
            }
            else {
                // Fall back for themes
                filteredDependencyDescriptors = dependencyDescriptors.ToList();
            }

            var entries = filteredDependencyDescriptors
                .SelectMany(descriptor => _loaders
                                              .Where(loader => descriptor.LoaderName == loader.Name)
                                              .Select(loader => new {
                                                  loader,
                                                  descriptor,
                                                  references = loader.GetCompilationReferences(descriptor),
                                                  dependencies = _extensionDependenciesManager.GetVirtualPathDependencies(descriptor.Name)
                                              }));

            // Add assemblies
            foreach (var entry in entries) {
                foreach (var reference in entry.references) {
                    if (!string.IsNullOrEmpty(reference.AssemblyName)) {
                        var assembly = _assemblyLoader.Load(reference.AssemblyName);
                        if (assembly != null)
                            assembliesToAdd.Add(assembly);
                    }
                    if (!string.IsNullOrEmpty(reference.BuildProviderTarget)) {
                        // Returned assembly may be null if the .csproj file doesn't containt any .cs file, for example
                        var assembly = _buildManager.GetCompiledAssembly(reference.BuildProviderTarget);
                        if (assembly != null)
                            assembliesToAdd.Add(assembly);
                    }
                }
            }

            foreach (var assembly in assembliesToAdd) {
                provider.AssemblyBuilder.AddAssemblyReference(assembly);
            }

            // Add virtual path dependencies (i.e. source files)
            //PERF: Ensure each virtual path is present only once in the list of dependencies
            var virtualDependencies = entries
                .SelectMany(e => e.dependencies)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var virtualDependency in virtualDependencies) {
                provider.AddVirtualPathDependency(virtualDependency);
            }

            // Logging
            if (Logger.IsEnabled(LogLevel.Debug)) {
                if (assembliesToAdd.Count == 0 && provider.VirtualPathDependencies == null) {
                    Logger.Debug("CodeGenerationStarted(\"{0}\") - no dependencies.", provider.VirtualPath);
                }
                else {
                    Logger.Debug("CodeGenerationStarted(\"{0}\") - Dependencies: ", provider.VirtualPath);
                    if (provider.VirtualPathDependencies != null) {
                        foreach (var virtualPath in provider.VirtualPathDependencies) {
                            Logger.Debug("  VirtualPath: \"{0}\"", virtualPath);
                        }
                    }
                    foreach (var assembly in assembliesToAdd) {
                        Logger.Debug("  Reference: \"{0}\"", assembly);
                    }
                }
            }
        }

        private DependencyDescriptor GetModuleDependencyDescriptor(string virtualPath) {
            var appRelativePath = VirtualPathUtility.ToAppRelative(virtualPath);
            var prefix = PrefixMatch(appRelativePath, new[] { "~/Modules/", "~/Core/" });
            if (prefix == null)
                return null;

            var moduleName = ModuleMatch(appRelativePath, prefix);
            if (moduleName == null)
                return null;

            return _dependenciesFolder.GetDescriptor(moduleName);
        }

        private static string ModuleMatch(string virtualPath, string prefix) {
            var index = virtualPath.IndexOf('/', prefix.Length, virtualPath.Length - prefix.Length);
            if (index < 0)
                return null;

            var moduleName = virtualPath.Substring(prefix.Length, index - prefix.Length);
            return (string.IsNullOrEmpty(moduleName) ? null : moduleName);
        }

        private static string PrefixMatch(string virtualPath, params string[] prefixes) {
            return prefixes
                .FirstOrDefault(p => virtualPath.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        public void CodeGenerationCompleted(RazorBuildProvider provider, CodeGenerationCompleteEventArgs e) {
        }
    }
}