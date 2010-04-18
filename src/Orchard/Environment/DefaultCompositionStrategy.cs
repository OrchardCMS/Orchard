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
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    //TEMP: This will be replaced by packaging system

    public interface ICompositionStrategy_Obsolete {
        IEnumerable<RecordDescriptor_Obsolete> GetRecordDescriptors_Obsolete();
    }

    public class RecordDescriptor_Obsolete {
        public Type Type { get; set; }
        public string Prefix { get; set; }
    }

    public class DefaultCompositionStrategy : ICompositionStrategy, ICompositionStrategy_Obsolete {
        private readonly IExtensionManager _extensionManager;

        public DefaultCompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public ShellTopology Compose(ShellTopologyDescriptor topologyDescriptor) {
            var featureDescriptors = _extensionManager.AvailableExtensions()
                .SelectMany(extensionDescriptor => extensionDescriptor.Features)
                .Where(featureDescriptor => IsFeatureEnabledInTopology(featureDescriptor, topologyDescriptor));

            var features = _extensionManager.LoadFeatures(featureDescriptors).Concat(CoreFeatures());

            return new ShellTopology {
                Modules = BuildTopology<ModuleTopology>(features, IsModule, BuildModule),
                Dependencies = BuildTopology<DependencyTopology>(features, IsDependency, (t, f) => BuildDependency(t, f, topologyDescriptor)),
                Controllers = BuildTopology<ControllerTopology>(features, IsController, BuildController),
                Records = BuildTopology<RecordTopology>(features, IsRecord, BuildRecord),
            };
        }

        private static IEnumerable<Feature> CoreFeatures() {
            var core = new Feature {
                Descriptor = new FeatureDescriptor {
                    Name = "Core",
                    Extension = new ExtensionDescriptor {
                        Name = "Core",
                        DisplayName = "Core",
                        AntiForgery = "enabled",
                    },
                },
                ExportedTypes = new[] {
                    typeof (ContentTypeRecord),
                    typeof (ContentItemRecord),
                    typeof (ContentItemVersionRecord),
                },
            };
            return new[] { core };
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
                   (!typeof(IContent).IsAssignableFrom(type) || typeof(ContentPartRecord).IsAssignableFrom(type)); ;
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


        public IEnumerable<RecordDescriptor_Obsolete> GetRecordDescriptors_Obsolete() {
            var descriptors = new List<RecordDescriptor_Obsolete>{
                new RecordDescriptor_Obsolete { Prefix = "Core", Type = typeof (ContentTypeRecord)},
                new RecordDescriptor_Obsolete { Prefix = "Core", Type = typeof (ContentItemRecord)},
                new RecordDescriptor_Obsolete { Prefix = "Core", Type = typeof (ContentItemVersionRecord)},
            };

            foreach (var extension in _extensionManager.ActiveExtensions_Obsolete()) {
                var prefix = extension.Descriptor.Name
                    .Replace("Orchard.", "")
                    .Replace(".", "_");

                var recordDescriptors = extension
                    .ExportedTypes
                    .Where(IsRecordType)
                    .Select(type => new RecordDescriptor_Obsolete { Prefix = prefix, Type = type });

                descriptors.AddRange(recordDescriptors);
            }

            return descriptors.ToReadOnlyCollection();
        }

        private static bool IsRecordType(Type type) {
            return ((type.Namespace ?? "").EndsWith(".Models") || (type.Namespace ?? "").EndsWith(".Records")) &&
                   type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors() ?? Enumerable.Empty<MethodInfo>()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                   (!typeof(IContent).IsAssignableFrom(type) || typeof(ContentPartRecord).IsAssignableFrom(type));
        }

    }
}