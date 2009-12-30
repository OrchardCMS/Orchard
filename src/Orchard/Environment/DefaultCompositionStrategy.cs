using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Extensions;

namespace Orchard.Environment {
    //TEMP: This will be replaced by packaging system

    public interface ICompositionStrategy {
        IEnumerable<Type> GetModuleTypes();
        IEnumerable<Type> GetDependencyTypes();
        IEnumerable<Type> GetRecordTypes();
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

        public IEnumerable<Type> GetRecordTypes() {
            var types = _extensionManager.ActiveExtensions().SelectMany(x => x.ExportedTypes);
            var coreRecords = new[] {
                                        typeof (ContentTypeRecord),
                                        typeof (ContentItemRecord),
                                        typeof (ContentItemVersionRecord),
                                    };
            var recordTypes = types.Where(IsRecordType).Concat(coreRecords);
            return recordTypes;
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