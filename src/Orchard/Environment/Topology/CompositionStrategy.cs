using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.Topology {
    /// <summary>
    /// Service at the host level to transform the cachable topology into the loadable topology.
    /// </summary>
    public interface ICompositionStrategy {
        /// <summary>
        /// Using information from the IExtensionManager, transforms and populates all of the
        /// topology model the shell builders will need to correctly initialize a tenant IoC container.
        /// </summary>
        ShellTopology Compose(ShellTopologyDescriptor descriptor);
    }

    public class CompositionStrategy : ICompositionStrategy {
        private readonly IExtensionManager _extensionManager;

        public CompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public ShellTopology Compose(ShellTopologyDescriptor topologyDescriptor) {
            var enabledFeatures = _extensionManager.AvailableExtensions()
                .SelectMany(extensionDescriptor => extensionDescriptor.Features)
                .Where(featureDescriptor => IsFeatureEnabledInTopology(featureDescriptor, topologyDescriptor));

            var features = _extensionManager.LoadFeatures(enabledFeatures);

            return new ShellTopology {
                Modules = BuildTopology<ModuleTopology>(features, IsModule, BuildModule),
                Dependencies = BuildTopology(features, IsDependency, (t, f) => BuildDependency(t, f, topologyDescriptor)),
                Controllers = BuildTopology<ControllerTopology>(features, IsController, BuildController),
                Records = BuildTopology<RecordTopology>(features, IsRecord, BuildRecord),
            };
        }



        private static bool IsFeatureEnabledInTopology(FeatureDescriptor featureDescriptor, ShellTopologyDescriptor topologyDescriptor) {
            return topologyDescriptor.EnabledFeatures.Any(topologyFeature => topologyFeature.Name == featureDescriptor.Name);
        }

        private static IEnumerable<T> BuildTopology<T>(
            IEnumerable<Feature> features,
            Func<Type, bool> predicate,
            Func<Type, Feature, T> selector) {
            return features.SelectMany(
                feature => feature.ExportedTypes
                               .Where(predicate)
                               .Select(type => selector(type, feature)))
                .ToArray();
        }

        private static bool IsModule(Type type) {
            return typeof(IModule).IsAssignableFrom(type);
        }

        private static ModuleTopology BuildModule(Type type, Feature feature) {
            return new ModuleTopology { Type = type, Feature = feature };
        }

        private static bool IsDependency(Type type) {
            return typeof(IDependency).IsAssignableFrom(type);
        }

        private static DependencyTopology BuildDependency(Type type, Feature feature, ShellTopologyDescriptor topologyDescriptor) {
            return new DependencyTopology {
                Type = type,
                Feature = feature,
                Parameters = topologyDescriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
            };
        }

        private static bool IsController(Type type) {
            return typeof(IController).IsAssignableFrom(type);
        }

        private static ControllerTopology BuildController(Type type, Feature feature) {
            var areaName = feature.Descriptor.Extension.Name;

            var controllerName = type.Name;
            if (controllerName.EndsWith("Controller"))
                controllerName = controllerName.Substring(0, controllerName.Length - "Controller".Length);

            return new ControllerTopology {
                Type = type,
                Feature = feature,
                AreaName = areaName,
                ControllerName = controllerName,
            };
        }

        private static bool IsRecord(Type type) {
            return ((type.Namespace ?? "").EndsWith(".Models") || (type.Namespace ?? "").EndsWith(".Records")) &&
                   type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors() ?? Enumerable.Empty<MethodInfo>()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                   (!typeof(IContent).IsAssignableFrom(type) || typeof(ContentPartRecord).IsAssignableFrom(type));
        }

        private static RecordTopology BuildRecord(Type type, Feature feature) {
            var extensionDescriptor = feature.Descriptor.Extension;
            var extensionName = extensionDescriptor.Name.Replace('.', '_');

            return new RecordTopology {
                Type = type,
                Feature = feature,
                TableName = extensionName + '_' + type.Name,
            };
        }
    }
}