using Autofac;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.ShellBuilders {
    public class DefaultShellContextFactory : IShellContextFactory {
        private readonly ITopologyDescriptorCache _topologyDescriptorCache;
        private readonly ICompositionStrategy _compositionStrategy;
        private readonly IShellContainerFactory _shellContainerFactory;

        public DefaultShellContextFactory(
            ITopologyDescriptorCache topologyDescriptorCache,
            ICompositionStrategy compositionStrategy,
            IShellContainerFactory shellContainerFactory) {
            _topologyDescriptorCache = topologyDescriptorCache;
            _compositionStrategy = compositionStrategy;
            _shellContainerFactory = shellContainerFactory;
        }

        public ShellContext Create(ShellSettings settings) {
            var cachedTopology = _topologyDescriptorCache.Fetch(settings.Name);
            // handle null-(e.g. cache miss)

            var topology = _compositionStrategy.Compose(cachedTopology);
            var shellScope = _shellContainerFactory.CreateContainer(topology);

            ShellTopologyDescriptor currentTopology;
            using (var standaloneEnvironment = new StandaloneEnvironment(shellScope)) {
                var topologyDescriptorProvider = standaloneEnvironment.Resolve<ITopologyDescriptorManager>();
                currentTopology = topologyDescriptorProvider.GetTopologyDescriptor();
            }

            if (cachedTopology.SerialNumber != currentTopology.SerialNumber) {
                _topologyDescriptorCache.Store(settings.Name, currentTopology);
                topology = _compositionStrategy.Compose(currentTopology);
                shellScope = _shellContainerFactory.CreateContainer(topology);
            }

            return new ShellContext {
                Settings = settings,
                TopologyDescriptor = currentTopology,
                Topology = topology,
                LifetimeScope = shellScope,
                Shell = shellScope.Resolve<IOrchardShell>(),
            };
        }
    }
}