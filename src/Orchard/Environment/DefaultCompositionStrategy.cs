using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Extensions;
using Orchard.Utility.Extensions;

namespace Orchard.Environment {
    //TEMP: This will be replaced by packaging system

    public interface ICompositionStrategy {
        IEnumerable<Type> GetModuleTypes();
        IEnumerable<Type> GetDependencyTypes();
        IEnumerable<RecordDescriptor> GetRecordDescriptors();
    }

    public class RecordDescriptor {
        public Type Type { get; set; }
        public string Prefix { get; set; }
    }

    public class DefaultCompositionStrategy : ICompositionStrategy {
        private readonly IExtensionManager _extensionManager;

        public DefaultCompositionStrategy(IExtensionManager extensionManager) {
            _extensionManager = extensionManager;
        }

        public IEnumerable<Type> GetModuleTypes() {
            var types = _extensionManager.ActiveExtensions().SelectMany(x => x.ExportedTypes);
            types = types.Concat(typeof(IOrchardHost).Assembly.GetExportedTypes());
            var nonAbstractClasses = types.Where(t => t.IsClass && !t.IsAbstract);
            var modules = nonAbstractClasses.Where(t => typeof(IModule).IsAssignableFrom(t));
            return modules;
        }

        public IEnumerable<Type> GetDependencyTypes() {
            var types = _extensionManager.ActiveExtensions().SelectMany(x => x.ExportedTypes);
            types = types.Concat(typeof(IOrchardHost).Assembly.GetExportedTypes());
            var nonAbstractClasses = types.Where(t => t.IsClass && !t.IsAbstract);
            var modules = nonAbstractClasses.Where(t => typeof(IDependency).IsAssignableFrom(t));
            return modules;
        }

        public IEnumerable<RecordDescriptor> GetRecordDescriptors() {
            var descriptors = new List<RecordDescriptor>{
                new RecordDescriptor { Prefix = "Core", Type = typeof (ContentTypeRecord)},
                new RecordDescriptor { Prefix = "Core", Type = typeof (ContentItemRecord)},
                new RecordDescriptor { Prefix = "Core", Type = typeof (ContentItemVersionRecord)},
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