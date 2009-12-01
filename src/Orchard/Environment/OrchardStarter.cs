using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Web;
using Autofac.Modules;
using Orchard.Packages;
using Orchard.Packages.Loaders;

namespace Orchard.Environment {
    public static class OrchardStarter {
        public static IContainer CreateHostContainer(Action<ContainerBuilder> registrations) {
            var builder = new ContainerBuilder();

            // Modifies the container to automatically return IEnumerable<T> of service type T
            builder.RegisterModule(new ImplicitCollectionSupportModule());

            // a single default host implementation is needed for bootstrapping a web app domain
            builder.Register<DefaultOrchardHost>().As<IOrchardHost>()
                .SingletonScoped();
            builder.Register<DefaultCompositionStrategy>().As<ICompositionStrategy>()
                .SingletonScoped();
            builder.Register<DefaultOrchardShell>().As<IOrchardShell>()
                .ContainerScoped().InContext("shell");

            // The container provider gives you access to the lowest container at the time, 
            // and dynamically creates a per-request container. The DisposeRequestContainer method
            // still needs to be called on end request, but that's the host component's job to worry about.
            builder.Register<ContainerProvider>().As<IContainerProvider>().ContainerScoped();

            builder.Register<PackageManager>().As<IPackageManager>().SingletonScoped();
            builder.Register<CorePackageLoader>().As<IPackageLoader>().SingletonScoped();
            builder.Register<ReferencedPackageLoader>().As<IPackageLoader>().SingletonScoped();
            builder.Register<PrecompiledPackageLoader>().As<IPackageLoader>().SingletonScoped();
            builder.Register<DynamicPackageLoader>().As<IPackageLoader>().SingletonScoped();

            //builder.Register((ctx, p) => new PackageFolders(MapPaths(p.Named<IEnumerable<string>>("paths"))))
            //    .As<IPackageFolders>()
            //    .WithExtendedProperty("paths", new[] { "~/Packages" })
            //    .SingletonScoped();
            builder.Register<PackageFolders>().As<IPackageFolders>()
                .WithArguments(new NamedParameter("paths", new[] { "~/Core", "~/Packages" }))
                .SingletonScoped();

            registrations(builder);

            return builder.Build();
        }

        public static IOrchardHost CreateHost(Action<ContainerBuilder> registrations) {
            return CreateHostContainer(registrations).Resolve<IOrchardHost>();
        }
    }
}
