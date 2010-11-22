using System.Collections.Generic;
using System.Linq;
using System.Web.Razor.Generator;
using System.Web.WebPages.Razor;
using Orchard.Environment;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Mvc.ViewEngines.Razor {
    public interface IRazorCompilationEvents {
        void CodeGenerationStarted(RazorBuildProvider provider);
        void CodeGenerationCompleted(RazorBuildProvider provider, CodeGenerationCompleteEventArgs e);
    }

    public class DefaultRazorCompilationEvents : IRazorCompilationEvents {
        private readonly IDependenciesFolder _dependenciesFolder;
        private readonly IBuildManager _buildManager;
        private readonly IEnumerable<IExtensionLoader> _loaders;
        private readonly IAssemblyLoader _assemblyLoader;

        public DefaultRazorCompilationEvents(IDependenciesFolder dependenciesFolder, IBuildManager buildManager, IEnumerable<IExtensionLoader> loaders, IAssemblyLoader assemblyLoader) {
            _dependenciesFolder = dependenciesFolder;
            _buildManager = buildManager;
            _loaders = loaders;
            _assemblyLoader = assemblyLoader;
        }

        public void CodeGenerationStarted(RazorBuildProvider provider) {
            var descriptors = _dependenciesFolder.LoadDescriptors();
            var entries = descriptors
                .SelectMany(descriptor => _loaders
                                              .Where(loader => descriptor.LoaderName == loader.Name)
                                              .Select(loader => new {
                                                  loader,
                                                  descriptor,
                                                  directive = loader.GetWebFormAssemblyDirective(descriptor),
                                                  dependencies = loader.GetWebFormVirtualDependencies(descriptor)
                                              }));

            foreach (var entry in entries) {
                if (entry.directive != null) {
                    if (entry.directive.StartsWith("<%@ Assembly Name=\"")) {
                        var assembly = _assemblyLoader.Load(entry.descriptor.Name);
                        if (assembly != null)
                            provider.AssemblyBuilder.AddAssemblyReference(assembly);
                    }
                    else if (entry.directive.StartsWith("<%@ Assembly Src=\"")) {
                        // Returned assembly may be null if the .csproj file doesn't containt any .cs file, for example
                        var assembly = _buildManager.GetCompiledAssembly(entry.descriptor.VirtualPath);
                        if (assembly != null)
                            provider.AssemblyBuilder.AddAssemblyReference(assembly);
                    }
                }
                foreach (var virtualDependency in entry.dependencies) {
                    provider.AddVirtualPathDependency(virtualDependency);
                }
            }
        }

        public void CodeGenerationCompleted(RazorBuildProvider provider, CodeGenerationCompleteEventArgs e) {
        }
    }
}