using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Autofac.Features.OwnedInstances;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using IModelBinderProvider = Orchard.Mvc.ModelBinders.IModelBinderProvider;

namespace Orchard.Environment {
    public class DefaultOrchardShell : IOrchardShell {
        private readonly Func<Owned<IOrchardShellEvents>> _eventsFactory;
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IModelBinderProvider> _modelBinderProviders;
        private readonly IModelBinderPublisher _modelBinderPublisher;
        private readonly ViewEngineCollection _viewEngines;

        public DefaultOrchardShell(
            Func<Owned<IOrchardShellEvents>> eventsFactory,
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IModelBinderProvider> modelBinderProviders,
            IModelBinderPublisher modelBinderPublisher,
            ViewEngineCollection viewEngines) {
            _eventsFactory = eventsFactory;
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _modelBinderProviders = modelBinderProviders;
            _modelBinderPublisher = modelBinderPublisher;
            _viewEngines = viewEngines;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Activate() {
            _routePublisher.Publish(_routeProviders.SelectMany(provider => provider.GetRoutes()));
            _modelBinderPublisher.Publish(_modelBinderProviders.SelectMany(provider => provider.GetModelBinders()));

            //AddOrchardLocationsFormats();

            using (var events = _eventsFactory()) {
                events.Value.Activated();
            }
        }

        public void Terminate() {
             using (var events = _eventsFactory()) {
                events.Value.Terminating();
            }
        }

        /// <summary>
        /// Adds view locations formats for non-themed views in custom orchard modules.
        /// </summary>
        private void AddOrchardLocationsFormats() {

            IEnumerable<string> orchardMasterLocationFormats = new[] {
                    "~/Modules/{2}/Views/{1}/{0}.master",
                    "~/Modules/{2}/Views/Shared/{0}.master",
                    "~/Themes/{2}/Views/{1}/{0}.master",
                    "~/Themes/{2}/Views/Shared/{0}.master",
                    "~/Core/{2}/Views/{1}/{0}.master",
                    "~/Core/{2}/Views/Shared/{0}.master",
                    "~/Areas/{2}/Views/{1}/{0}.master",
                    "~/Areas/{2}/Views/Shared/{0}.master",
                };

            IEnumerable<string> orchardLocationFormats = new[] {
                    "~/Modules/{2}/Views/{1}/{0}.aspx",
                    "~/Modules/{2}/Views/{1}/{0}.ascx",
                    "~/Modules/{2}/Views/Shared/{0}.aspx",
                    "~/Modules/{2}/Views/Shared/{0}.ascx",
                    "~/Themes/{2}/Views/{1}/{0}.aspx",
                    "~/Themes/{2}/Views/{1}/{0}.ascx",
                    "~/Themes/{2}/Views/Shared/{0}.aspx",
                    "~/Themes/{2}/Views/Shared/{0}.ascx",
                    "~/Core/{2}/Views/{1}/{0}.aspx",
                    "~/Core/{2}/Views/{1}/{0}.ascx",
                    "~/Core/{2}/Views/Shared/{0}.aspx",
                    "~/Core/{2}/Views/Shared/{0}.ascx",
                    "~/Areas/{2}/Views/{1}/{0}.aspx",
                    "~/Areas/{2}/Views/{1}/{0}.ascx",
                    "~/Areas/{2}/Views/Shared/{0}.aspx",
                    "~/Areas/{2}/Views/Shared/{0}.ascx",
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

        private static string ModelsLocationFormat(ExtensionDescriptor descriptor) {
            return Path.Combine(Path.Combine(descriptor.Location, descriptor.Name), "Views/Shared/{0}.ascx");
        }



    }
}
