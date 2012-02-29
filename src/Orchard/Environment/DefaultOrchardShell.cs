using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.OwnedInstances;
using Orchard.Logging;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using Orchard.Tasks;
using IModelBinderProvider = Orchard.Mvc.ModelBinders.IModelBinderProvider;

namespace Orchard.Environment {
    public class DefaultOrchardShell : IOrchardShell {
        private readonly Func<Owned<IOrchardShellEvents>> _eventsFactory;
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IModelBinderProvider> _modelBinderProviders;
        private readonly IModelBinderPublisher _modelBinderPublisher;
        private readonly ISweepGenerator _sweepGenerator;

        public DefaultOrchardShell(
            Func<Owned<IOrchardShellEvents>> eventsFactory,
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IModelBinderProvider> modelBinderProviders,
            IModelBinderPublisher modelBinderPublisher,
            ISweepGenerator sweepGenerator) {
            _eventsFactory = eventsFactory;
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _modelBinderProviders = modelBinderProviders;
            _modelBinderPublisher = modelBinderPublisher;
            _sweepGenerator = sweepGenerator;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Activate() {
            _routePublisher.Publish(_routeProviders.SelectMany(provider => provider.GetRoutes()));
            _modelBinderPublisher.Publish(_modelBinderProviders.SelectMany(provider => provider.GetModelBinders()));

            _sweepGenerator.Activate();

            using (var events = _eventsFactory()) {
                events.Value.Activated();
            }
        }

        public void Terminate() {
            using (var events = _eventsFactory()) {
                try {
                    events.Value.Terminating();
                }
                catch {
                    // ignore exceptions while terminating the application
                }

                _sweepGenerator.Terminate();
            }
        }
    }
}
