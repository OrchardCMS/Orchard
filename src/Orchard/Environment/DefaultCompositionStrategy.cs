using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using Autofac;
using Orchard.Packages;

namespace Orchard.Environment {
    //TEMP: This will be replaced by packaging system

    public interface ICompositionStrategy {
        IEnumerable<Assembly> GetAssemblies();
        IEnumerable<Type> GetModuleTypes();
        IEnumerable<Type> GetDependencyTypes();
    }

    public class DefaultCompositionStrategy : ICompositionStrategy {
        private readonly IPackageManager _packageManager;

        public DefaultCompositionStrategy(IPackageManager packageManager) {
            _packageManager = packageManager;
        }

        public IEnumerable<Assembly> GetAssemblies() {
            return _packageManager.ActivePackages()
                .Select(entry => entry.Assembly)
                .Concat(new[]{typeof(IOrchardHost).Assembly});
            //return BuildManager.GetReferencedAssemblies().OfType<Assembly>();
        }

        public IEnumerable<Type> GetModuleTypes() {
            var assemblies = GetAssemblies();
            var notFromAutofac = assemblies.Where(a => !IsAutofacAssembly(a));
            var types = notFromAutofac.SelectMany(a => a.GetExportedTypes());
            var nonAbstractClasses = types.Where(t => t.IsClass && !t.IsAbstract);
            var modules = nonAbstractClasses.Where(t => typeof(IModule).IsAssignableFrom(t));
            return modules;
        }

        private static bool IsAutofacAssembly(Assembly assembly) {
            return assembly == typeof(Autofac.IModule).Assembly ||
                   assembly == typeof(Autofac.Integration.Web.IContainerProvider).Assembly;
        }

        public IEnumerable<Type> GetDependencyTypes() {
            var assemblies = GetAssemblies();
            var types = assemblies.SelectMany(a => a.GetExportedTypes());
            var nonAbstractClasses = types.Where(t => t.IsClass && !t.IsAbstract);
            var modules = nonAbstractClasses.Where(t => typeof(IDependency).IsAssignableFrom(t));
            return modules;
        }
    }
}