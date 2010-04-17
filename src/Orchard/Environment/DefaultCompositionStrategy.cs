using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Extensions;
using Orchard.Extensions.Models;
using Orchard.Utility.Extensions;
using Orchard.Extensions.Records;

namespace Orchard.Environment {
    //TEMP: This will be replaced by packaging system

    public interface ICompositionStrategy_Obsolete {
        IEnumerable<Type> GetModuleTypes();
        IEnumerable<Type> GetDependencyTypes();
        IEnumerable<RecordDescriptor> GetRecordDescriptors();
    }

    public class RecordDescriptor {
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

            var features = _extensionManager.LoadFeatures(featureDescriptors);

            return new ShellTopology {
                Modules = BuildTopology<ModuleTopology>(features, IsModule, BuildModule),
                Dependencies = BuildTopology<DependencyTopology>(features, IsDependency, BuildDependency),
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

        private static DependencyTopology BuildDependency(Type type, Feature feature) {
            return new DependencyTopology {Type = type, Feature = feature};
        }

        private static bool IsController(Type type) {
            return typeof(IController).IsAssignableFrom(type);
        }

        private static ControllerTopology BuildController(Type type, Feature feature) {
            return new ControllerTopology { Type = type, Feature = feature };
        }

        private static bool IsRecord(Type type) {
            return ((type.Namespace ?? "").EndsWith(".Models") || (type.Namespace ?? "").EndsWith(".Records")) &&
                   type.GetProperty("Id") != null &&
                   (type.GetProperty("Id").GetAccessors() ?? Enumerable.Empty<MethodInfo>()).All(x => x.IsVirtual) &&
                   !type.IsSealed &&
                   !type.IsAbstract &&
                   (!typeof(IContent).IsAssignableFrom(type) || typeof(ContentPartRecord).IsAssignableFrom(type));;
        }

        private static RecordTopology BuildRecord(Type type, Feature feature) {
            return new RecordTopology { Type = type, Feature = feature };
        }


        public IEnumerable<Type> GetModuleTypes() {
            return _extensionManager.GetExtensionsTopology().Types.Where(t => typeof(IModule).IsAssignableFrom(t));
        }

        public IEnumerable<Type> GetDependencyTypes() {
            return _extensionManager.GetExtensionsTopology().Types.Where(t => typeof(IDependency).IsAssignableFrom(t));
        }

        public IEnumerable<RecordDescriptor> GetRecordDescriptors() {
            var descriptors = new List<RecordDescriptor>{
                new RecordDescriptor { Prefix = "Core", Type = typeof (ContentTypeRecord)},
                new RecordDescriptor { Prefix = "Core", Type = typeof (ContentItemRecord)},
                new RecordDescriptor { Prefix = "Core", Type = typeof (ContentItemVersionRecord)},
                new RecordDescriptor { Prefix = "Core", Type = typeof (ExtensionRecord)},
            };

            foreach (var extension in _extensionManager.ActiveExtensions()) {
                var prefix = extension.Descriptor.Name
                    .Replace("Orchard.", "")
                    .Replace(".", "_");

                var recordDescriptors = extension
                    .ExportedTypes
                    .Where(IsRecordType)
                    .Select(type => new RecordDescriptor { Prefix = prefix, Type = type });

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