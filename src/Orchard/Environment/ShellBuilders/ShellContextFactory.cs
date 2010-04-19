using System.Linq;
using Autofac;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Logging;

namespace Orchard.Environment.ShellBuilders {

    public interface IShellContextFactory {
        ShellContext Create(ShellSettings settings);
    }

    public class ShellContextFactory : IShellContextFactory {
        private readonly IShellDescriptorCache _shellDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContextFactory(
            IShellDescriptorCache shellDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _shellDescriptorCache = shellDescriptorCache;
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public ShellContext Create(ShellSettings settings) {
            if (settings == null) {
                return CreateSetupContext();
            }

            Logger.Debug("Creating shell context for tenant {0}", settings.Name);

            var knownDescriptor = _shellDescriptorCache.Fetch(settings.Name);
            if (knownDescriptor == null) {
                Logger.Information("No topology cached. Starting with minimum components.");
                knownDescriptor = MinimumTopologyDescriptor();
            }

            var topology = _compositionStrategy.Compose(knownDescriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, topology);

            ShellDescriptor currentDescriptor;
            using (var standaloneEnvironment = new StandaloneEnvironment(shellScope)) {
                var topologyDescriptorProvider = standaloneEnvironment.Resolve<IShellDescriptorManager>();
                currentDescriptor = topologyDescriptorProvider.GetShellDescriptor();
            }

            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber) {
                Logger.Information("Newer topology obtained. Rebuilding shell container.");

                _shellDescriptorCache.Store(settings.Name, currentDescriptor);
                topology = _compositionStrategy.Compose(currentDescriptor);
                shellScope = _shellContainerFactory.CreateContainer(settings, topology);
            }

            return new ShellContext {
                Settings = settings,
                Descriptor = currentDescriptor,
                Topology = topology,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }

        private static ShellDescriptor MinimumTopologyDescriptor() {
            return new ShellDescriptor {
                SerialNumber = -1,
                EnabledFeatures = new[] {
                    new ShellFeature {Name = "Orchard.Framework"},
                    new ShellFeature {Name = "Settings"},
                },
                Parameters = Enumerable.Empty<ShellParameter>(),
            };
        }

        private ShellContext CreateSetupContext() {
            Logger.Warning("No shell settings available. Creating shell context for setup");

            var settings = new ShellSettings { Name = "__Orchard__Setup__" };
            var descriptor = new ShellDescriptor {
                SerialNumber = -1,
                EnabledFeatures = new[] { new ShellFeature { Name = "Orchard.Setup" } },
            };

            var topology = _compositionStrategy.Compose(descriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, topology);

            return new ShellContext {
                Settings = settings,
                Descriptor = descriptor,
                Topology = topology,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }
    }
}