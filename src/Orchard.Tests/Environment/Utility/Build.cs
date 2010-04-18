using System.Linq;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Topology.Models;

namespace Orchard.Tests.Environment.Utility {
    static class Build {

        public static ShellTopologyDescriptor TopologyDescriptor() {
            var descriptor = new ShellTopologyDescriptor {
                                                             EnabledFeatures = Enumerable.Empty<TopologyFeature>(),
                                                             Parameters = Enumerable.Empty<TopologyParameter>(),
                                                         };
            return descriptor;
        }

        public static ShellTopologyDescriptor WithFeatures(this ShellTopologyDescriptor descriptor, params string[] names) {
            descriptor.EnabledFeatures = descriptor.EnabledFeatures.Concat(
                names.Select(name => new TopologyFeature { Name = name }));

            return descriptor;
        }

        public static ShellTopologyDescriptor WithParameter<TComponent>(this ShellTopologyDescriptor descriptor, string name, string value) {
            descriptor.Parameters = descriptor.Parameters.Concat(
                new[] { new TopologyParameter { Component = typeof(TComponent).FullName, Name = name, Value = value } });

            return descriptor;
        }

        public static ExtensionDescriptor ExtensionDescriptor(string name, string displayName) {
            var descriptor = new ExtensionDescriptor {
                                                         Name = name,
                                                         DisplayName = displayName,
                                                         Features = Enumerable.Empty<FeatureDescriptor>(),
                                                     };
            return descriptor;
        }

        public static ExtensionDescriptor ExtensionDescriptor(string name) {
            return ExtensionDescriptor(name, name);
        }

        public static ExtensionDescriptor WithFeatures(this ExtensionDescriptor descriptor, params string[] names) {
            descriptor.Features = descriptor.Features.Concat(
                names.Select(name => new FeatureDescriptor { Extension=descriptor , Name = name, }));

            return descriptor;
        }
    }
}