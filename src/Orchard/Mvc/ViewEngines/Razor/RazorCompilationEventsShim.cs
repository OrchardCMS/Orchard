using System;
using System.Collections.Generic;
using System.Linq;
using System.Razor.Web;
using System.Reflection;
using System.Threading;
using Orchard.Environment;
using Orchard.Environment.Extensions.Loaders;
using Orchard.FileSystems.Dependencies;

namespace Orchard.Mvc.ViewEngines.Razor {
    public class RazorCompilationEventsShim : IShim {
        private static int _initialized;
        private IOrchardHostContainer _hostContainer;
        private static RazorCompilationEventsShim _instance;

        private RazorCompilationEventsShim() {
            RazorBuildProvider.CodeGenerationStarted += new EventHandler(RazorBuildProvider_CodeGenerationStarted);
            OrchardHostContainerRegistry.RegisterShim(this);
        }

        void RazorBuildProvider_CodeGenerationStarted(object sender, EventArgs e) {
            var provider = (RazorBuildProvider)sender;

            var descriptors = DependenciesFolder.LoadDescriptors();
            var entries = descriptors
                .SelectMany(descriptor => Loaders
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
                        provider.AssemblyBuilder.AddAssemblyReference(Assembly.Load(entry.descriptor.Name));
                    }
                    else if (entry.directive.StartsWith("<%@ Assembly Src=\"")) {
                        provider.AssemblyBuilder.AddAssemblyReference(BuildManager.GetCompiledAssembly(entry.descriptor.VirtualPath));
                    }
                }
                foreach (var virtualDependency in entry.dependencies) {
                    provider.AddVirtualPathDependency(virtualDependency);
                }
            }
        }


        public IOrchardHostContainer HostContainer {
            get { return _hostContainer; }
            set {
                _hostContainer = value;
                BuildManager = _hostContainer.Resolve<IBuildManager>();
                DependenciesFolder = _hostContainer.Resolve<IDependenciesFolder>();
                Loaders = _hostContainer.Resolve<IEnumerable<IExtensionLoader>>();
            }
        }

        public IBuildManager BuildManager { get; set; }
        public IDependenciesFolder DependenciesFolder { get; set; }
        public IEnumerable<IExtensionLoader> Loaders { get; set; }



        public static void EnsureInitialized() {
            var uninitialized = Interlocked.CompareExchange(ref _initialized, 1, 0) == 0;
            if (uninitialized)
                _instance = new RazorCompilationEventsShim();
        }
    }
}
