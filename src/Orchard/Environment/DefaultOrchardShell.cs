using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Orchard.Logging;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Extensions;
using Orchard.Utility;

namespace Orchard.Environment {
    public class DefaultOrchardShell : IOrchardShell {
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IModelBinderProvider> _modelBinderProviders;
        private readonly IModelBinderPublisher _modelBinderPublisher;
        private readonly ViewEngineCollection _viewEngines;
        private readonly IEnumerable<IOrchardShellEvents> _events;

        public DefaultOrchardShell(
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IModelBinderProvider> modelBinderProviders,
            IModelBinderPublisher modelBinderPublisher,
            ViewEngineCollection viewEngines,
            IEnumerable<IOrchardShellEvents> events) {
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _modelBinderProviders = modelBinderProviders;
            _modelBinderPublisher = modelBinderPublisher;
            _viewEngines = viewEngines;
            _events = events;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Activate() {
            _routePublisher.Publish(_routeProviders.SelectMany(provider => provider.GetRoutes()));
            _modelBinderPublisher.Publish(_modelBinderProviders.SelectMany(provider => provider.GetModelBinders()));

            AddOrchardLocationsFormats();

            _events.Invoke(x => x.Activated(), Logger);
        }

        /// <summary>
        /// Adds view locations formats for non-themed views in custom orchard modules.
        /// </summary>
        private void AddOrchardLocationsFormats() {

            IEnumerable<string> orchardMasterLocationFormats = new[] {
                    "~/Packages/{2}/Views/{1}/{0}.master",
                    "~/Packages/{2}/Views/Shared/{0}.master",
                };

            IEnumerable<string> orchardLocationFormats = new[] {
                    "~/Packages/{2}/Views/{1}/{0}.aspx",
                    "~/Packages/{2}/Views/{1}/{0}.ascx",
                    "~/Packages/{2}/Views/Shared/{0}.aspx",
                    "~/Packages/{2}/Views/Shared/{0}.ascx",
                };

            var viewEngine = _viewEngines.OfType<VirtualPathProviderViewEngine>().Single();
            viewEngine.AreaMasterLocationFormats = orchardMasterLocationFormats
                .Concat(viewEngine.AreaMasterLocationFormats)
                .Distinct()
                .ToArray();
            viewEngine.AreaViewLocationFormats = orchardLocationFormats
                .Concat(viewEngine.AreaViewLocationFormats)
                .Distinct()
                .ToArray();
            viewEngine.AreaPartialViewLocationFormats = orchardLocationFormats
                .Concat(viewEngine.AreaPartialViewLocationFormats)
                .Distinct()
                .ToArray();
        }


        public void Terminate() {
            _events.Invoke(x => x.Terminating(), Logger);
        }


        private static string ModelsLocationFormat(ExtensionDescriptor descriptor) {
            return Path.Combine(Path.Combine(descriptor.Location, descriptor.Name), "Views/Shared/{0}.ascx");
        }
    }
}