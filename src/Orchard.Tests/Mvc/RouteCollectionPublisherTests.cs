using System;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Mvc.Routes;
using Orchard.Tests.Utility;

namespace Orchard.Tests.Mvc {
    [TestFixture]
    public class RouteCollectionPublisherTests {
        private IContainer _container;
        private RouteCollection _routes;

        static RouteDescriptor Desc(string name, string url) {
            return new RouteDescriptor { Name = name, Route = new Route(url, new MvcRouteHandler()) };
        }

        [SetUp]
        public void Init() {
            _routes = new RouteCollection();

            var builder = new ContainerBuilder();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>();
            builder.RegisterType<ShellRoute>().InstancePerDependency();
            builder.Register(ctx => _routes);
            builder.Register(ctx => new ShellSettings { Name = ShellSettings.DefaultName });
            builder.RegisterAutoMocking();
            _container = builder.Build();
        }

        [Test]
        public void RoutesCanHaveNullOrEmptyNames() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            publisher.Publish(new[] { Desc(null, "bar"), Desc(string.Empty, "quux") });

            Assert.That(_routes.Count(), Is.EqualTo(3));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SameNameTwiceCausesExplosion() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            publisher.Publish(new[] { Desc("yarg", "bar"), Desc("yarg", "quux") });

            Assert.That(_routes.Count(), Is.EqualTo(2));
        }


        [Test]
        public void ExplosionLeavesOriginalRoutesIntact() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            try {
                publisher.Publish(new[] { Desc("yarg", "bar"), Desc("yarg", "quux") });
            }
            catch (ArgumentException) {
                Assert.That(_routes.Count(), Is.EqualTo(1));
                Assert.That(_routes.OfType<Route>().Single().Url, Is.EqualTo("{controller}"));
            }
        }

        [Test]
        public void WriteBlocksWhileReadIsInEffect() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();

            var readLock = _routes.GetReadLock();

            string where = "init";
            var action = new Action(() => {
                where = "before";
                publisher.Publish(new[] { Desc("barname", "bar"), Desc("quuxname", "quux") });
                where = "after";
            });

            Assert.That(where, Is.EqualTo("init"));
            var asyncResult = action.BeginInvoke(null, null);
            Thread.Sleep(75);
            Assert.That(where, Is.EqualTo("before"));
            readLock.Dispose();
            Thread.Sleep(75);
            Assert.That(where, Is.EqualTo("after"));
            action.EndInvoke(asyncResult);
        }

        [Test]
        public void RouteDescriptorWithNameCreatesNamedRouteInCollection() {
            _routes.MapRoute("foo", "{controller}");

            var publisher = _container.Resolve<IRoutePublisher>();
            var routeDescriptor = Desc("yarg", "bar");
            publisher.Publish(new[] { routeDescriptor });

            Assert.That(_routes["yarg"], Is.Not.Null);
        }
    }
}

