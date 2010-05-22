using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Configuration;
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
        ShellTopology Compose(ShellSettings settings, ShellDescriptor descriptor);
    }

    public class CompositionStrategy : ICompositionStrategy {
        private readonly IExtensionManager _extensionManager;

        public CompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public ShellTopology Compose(ShellSettings settings, ShellDescriptor descriptor) {
            var enabledFeatures = _extensionManager.AvailableExtensions()
                .SelectMany(extensionDescriptor => extensionDescriptor.Features)
                .Where(featureDescriptor => IsFeatureEnabledInTopology(featureDescriptor, descriptor));

            var features = _extensionManager.LoadFeatures(enabledFeatures);

            if (descriptor.EnabledFeatures.Any(feature => feature.Name == "Orchard.Framework"))
                features = features.Concat(BuiltinFeatures());

            var modules = BuildTopology<DependencyTopology>(features, IsModule, BuildModule);
            var dependencies = BuildTopology(features, IsDependency, (t, f) => BuildDependency(t, f, descriptor));
            var controllers = BuildTopology<ControllerTopology>(features, IsController, BuildController);
            var records = BuildTopology(features, IsRecord, (t, f) => BuildRecord(t, f, settings));

            return new ShellTopology {
                Dependencies = dependencies.Concat(modules).ToArray(),
                Controllers = controllers,
                Records = records,
            };
        }

        private static bool IsFeatureEnabledInTopology(FeatureDescriptor featureDescriptor, ShellDescriptor descriptor) {
            return descriptor.EnabledFeatures.Any(topologyFeature => topologyFeature.Name == featureDescriptor.Name);
        }

        private static IEnumerable<Feature> BuiltinFeatures() {
            yield return new Feature {
                Descriptor = new FeatureDescriptor {
                    Name = "Orchard.Framework",
                    Extension = new ExtensionDescriptor {
                        Name = "Orchard.Framework"
                    }
                },
                ExportedTypes =
                    typeof(OrchardStarter).Assembly.GetExportedTypes()
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Except(new[] { typeof(DefaultOrchardHost) })
                    .ToArray()
            };
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

        private static DependencyTopology BuildModule(Type type, Feature feature) {
            return new DependencyTopology { Type = type, Feature = feature, Parameters = Enumerable.Empty<ShellParameter>() };
        }

        private static bool IsDependency(Type type) {
            return typeof(IDependency).IsAssignableFrom(type);
        }

        private static DependencyTopology BuildDependency(Type type, Feature feature, ShellDescriptor descriptor) {
            return new DependencyTopology {
                Type = type,
                Feature = feature,
                Parameters = descriptor.Parameters.Where(x => x.Component == type.FullName).ToArray()
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

        private static RecordTopology BuildRecord(Type type, Feature feature, ShellSettings settings) {
            var extensionDescriptor = feature.Descriptor.Extension;
            var extensionName = extensionDescriptor.Name.Replace('.', '_');

            var dataTablePrefix = "";
            if (!string.IsNullOrEmpty(settings.DataTablePrefix))
                dataTablePrefix = settings.DataTablePrefix + "_";

            return new RecordTopology {
                Type = type,
                Feature = feature,
                TableName = dataTablePrefix + extensionName + '_' + type.Name,
            };
        }
    }
}