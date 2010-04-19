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
        private readonly ITopologyDescriptorCache _topologyDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public ShellContextFactory(
            ITopologyDescriptorCache topologyDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _topologyDescriptorCache = topologyDescriptorCache;
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

            var knownDescriptor = _topologyDescriptorCache.Fetch(settings.Name);
            if (knownDescriptor == null) {
                Logger.Information("No topology cached. Starting with minimum components.");
                knownDescriptor = MinimumTopologyDescriptor();
            }

            var topology = _compositionStrategy.Compose(knownDescriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, topology);

            ShellTopologyDescriptor currentDescriptor;
            using (var standaloneEnvironment = new StandaloneEnvironment(shellScope)) {
                var topologyDescriptorProvider = standaloneEnvironment.Resolve<ITopologyDescriptorManager>();
                currentDescriptor = topologyDescriptorProvider.GetTopologyDescriptor();
            }

            if (currentDescriptor != null && knownDescriptor.SerialNumber != currentDescriptor.SerialNumber) {
                Logger.Information("Newer topology obtained. Rebuilding shell container.");

                _topologyDescriptorCache.Store(settings.Name, currentDescriptor);
                topology = _compositionStrategy.Compose(currentDescriptor);
                shellScope = _shellContainerFactory.CreateContainer(settings, topology);
            }

            return new ShellContext {
                Settings = settings,
                TopologyDescriptor = currentDescriptor,
                Topology = topology,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }

        private static ShellTopologyDescriptor MinimumTopologyDescriptor() {
            return new ShellTopologyDescriptor {
                SerialNumber = -1,
                EnabledFeatures = new[] {
                    new TopologyFeature {Name = "Orchard.Framework"},
                    new TopologyFeature {Name = "Settings"},
                },
                Parameters = Enumerable.Empty<TopologyParameter>(),
            };
        }

        private ShellContext CreateSetupContext() {
            Logger.Warning("No shell settings available. Creating shell context for setup");

            var settings = new ShellSettings { Name = "__Orchard__Setup__" };
            var descriptor = new ShellTopologyDescriptor {
                SerialNumber = -1,
                EnabledFeatures = new[] { new TopologyFeature { Name = "Orchard.Setup" } },
            };

            var topology = _compositionStrategy.Compose(descriptor);
            var shellScope = _shellContainerFactory.CreateContainer(settings, topology);

            return new ShellContext {
                Settings = settings,
                TopologyDescriptor = descriptor,
                Topology = topology,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }
    }
}