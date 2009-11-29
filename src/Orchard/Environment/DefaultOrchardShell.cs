using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Packages;

namespace Orchard.Environment {
    public class DefaultOrchardShell : IOrchardShell {
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IModelBinderProvider> _modelBinderProviders;
        private readonly IModelBinderPublisher _modelBinderPublisher;
        private readonly ViewEngineCollection _viewEngines;
        private readonly IPackageManager _packageManager;
        private readonly IEnumerable<IOrchardShellEvents> _events;

        public DefaultOrchardShell(
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IModelBinderProvider> modelBinderProviders,
            IModelBinderPublisher modelBinderPublisher,
            ViewEngineCollection viewEngines,
            IPackageManager packageManager,
            IEnumerable<IOrchardShellEvents> events) {
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _modelBinderProviders = modelBinderProviders;
            _modelBinderPublisher = modelBinderPublisher;
            _viewEngines = viewEngines;
            _packageManager = packageManager;
            _events = events;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        static IEnumerable<string> OrchardLocationFormats() {
            return new[] {
                "~/Packages/{2}/Views/{1}/{0}.aspx",
                "~/Packages/{2}/Views/{1}/{0}.ascx",
                "~/Packages/{2}/Views/Shared/{0}.aspx",
                "~/Packages/{2}/Views/Shared/{0}.ascx",
                "~/Core/{2}/Views/{1}/{0}.aspx",
                "~/Core/{2}/Views/{1}/{0}.ascx",
                "~/Core/{2}/Views/Shared/{0}.aspx",
                "~/Core/{2}/Views/Shared/{0}.ascx",
            };
        }

        public void Activate() {
            _routePublisher.Publish(_routeProviders.SelectMany(provider => provider.GetRoutes()));
            _modelBinderPublisher.Publish(_modelBinderProviders.SelectMany(provider => provider.GetModelBinders()));


            var viewEngine = _viewEngines.OfType<VirtualPathProviderViewEngine>().Single();
            viewEngine.AreaViewLocationFormats = OrchardLocationFormats()
                .Concat(viewEngine.AreaViewLocationFormats)
                .Distinct()
                .ToArray();
            viewEngine.AreaPartialViewLocationFormats = OrchardLocationFormats()
                .Concat(viewEngine.AreaPartialViewLocationFormats)
                .Distinct()
                .ToArray();

            var activePackageDescriptors = _packageManager.ActivePackages().Select(x => x.Descriptor);
            var sharedLocationFormats = activePackageDescriptors.Select(x => ModelsLocationFormat(x));
            viewEngine.PartialViewLocationFormats = sharedLocationFormats
                .Concat(viewEngine.PartialViewLocationFormats)
                .Distinct()
                .ToArray();

            _events.Invoke(x => x.Activated(), Logger);
        }


        public void Terminate() {
            _events.Invoke(x => x.Terminating(), Logger);
        }


        private static string ModelsLocationFormat(PackageDescriptor descriptor) {
            return Path.Combine(Path.Combine(descriptor.Location, descriptor.Name), "Views/Models/{0}.ascx");
        }
    }
}