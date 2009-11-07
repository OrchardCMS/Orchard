using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Compilation;
using Autofac;

namespace Orchard.Environment {
    //TEMP: This will be replaced by packaging system

    public interface ICompositionStrategy {
        IEnumerable<Assembly> GetAssemblies();
        IEnumerable<Type> GetModuleTypes();
        IEnumerable<Type> GetDependencyTypes();
    }

    public class DefaultCompositionStrategy : ICompositionStrategy {
        public IEnumerable<Assembly> GetAssemblies() {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>();
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