using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.Mvc.ModelBinders;
using Orchard.Mvc.Routes;
using IModelBinderProvider = Orchard.Mvc.ModelBinders.IModelBinderProvider;

namespace Orchard.Tests.Environment {
    [TestFixture]
    public class DefaultOrchardShellTests {
        static RouteDescriptor Desc(string name, string url) {
            return new RouteDescriptor { Name = name, Route = new Route(url, new MvcRouteHandler()) };
        }

        static ModelBinderDescriptor BinderDesc(Type type, IModelBinder modelBinder) {
            return new ModelBinderDescriptor { Type = type, ModelBinder = modelBinder };
        }

        //[Test]
        //public void ActivatingRuntimeCausesRoutesAndModelBindersToBePublished() {

        //    var provider1 = new StubRouteProvider(new[] { Desc("foo1", "foo1"), Desc("foo2", "foo2") });
        //    var provider2 = new StubRouteProvider(new[] { Desc("foo1", "foo1"), Desc("foo2", "foo2") });
        //    var publisher = new StubRoutePublisher();

        //    var modelBinderProvider1 = new StubModelBinderProvider(new[] { BinderDesc(typeof(object), null), BinderDesc(typeof(string), null) });
        //    var modelBinderProvider2 = new StubModelBinderProvider(new[] { BinderDesc(typeof(int), null), BinderDesc(typeof(long), null) });
        //    var modelBinderPublisher = new StubModelBinderPublisher();

        //    var runtime = new DefaultOrchardShell(
        //        new[] { provider1, provider2 },
        //        publisher,
        //        new[] { modelBinderProvider1, modelBinderProvider2 },
        //        modelBinderPublisher,
        //        new ViewEngineCollection { new WebFormViewEngine() },
        //        new Mock<IOrchardShellEvents>().Object);

        //    runtime.Activate();

        //    Assert.That(publisher.Routes.Count(), Is.EqualTo(4));
        //    Assert.That(modelBinderPublisher.ModelBinders.Count(), Is.EqualTo(4));
        //}

        public class StubRouteProvider : IRouteProvider {
            private readonly IEnumerable<RouteDescriptor> _routes;

            public StubRouteProvider(IEnumerable<RouteDescriptor> routes) {
                _routes = routes;
            }

            public IEnumerable<RouteDescriptor> GetRoutes() {
                return _routes;
            }

            public void GetRoutes(ICollection<RouteDescriptor> routes) {
                foreach (var routeDescriptor in GetRoutes())
                    routes.Add(routeDescriptor);
            }
        }

        public class StubRoutePublisher : IRoutePublisher {
            public void Publish(IEnumerable<RouteDescriptor> routes) {
                Routes = routes;
            }
            public IEnumerable<RouteDescriptor> Routes { get; set; }
            public void Publish(IEnumerable<RouteDescriptor> routes, Func<IDictionary<string, object>, Task> pipeline) {
            }
        }

        public class StubModelBinderProvider : IModelBinderProvider {
            private readonly IEnumerable<ModelBinderDescriptor> _binders;

            public StubModelBinderProvider(IEnumerable<ModelBinderDescriptor> routes) {
                _binders = routes;
            }

            public IEnumerable<ModelBinderDescriptor> GetModelBinders() {
                return _binders;
            }
        }

        public class StubModelBinderPublisher : IModelBinderPublisher {
            public void Publish(IEnumerable<ModelBinderDescriptor> modelBinders) {
                ModelBinders = modelBinders;
            }
            public IEnumerable<ModelBinderDescriptor> ModelBinders { get; set; }
        }
    }
}
