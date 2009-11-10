using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.Mvc.Routes;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Mvc {
    [TestFixture]
    public class RouteCollectionPublisherTests {
        static RouteDescriptor Desc(string name, string url) {
            return new RouteDescriptor {Name = name, Route = new Route(url, new MvcRouteHandler())};
        }

        [Test]
        public void PublisherShouldReplaceRoutes() {

            var routes = new RouteCollection();
            routes.MapRoute("foo", "{controller}");

            IRoutePublisher publisher = new RoutePublisher(routes, new StubContainerProvider(null, null));
            publisher.Publish(new[] {Desc("barname", "bar"), Desc("quuxname", "quux")});

            Assert.That(routes.Count(), Is.EqualTo(2));
        }

        [Test]
        public void RoutesCanHaveNullOrEmptyNames() {
            var routes = new RouteCollection();
            routes.MapRoute("foo", "{controller}");

            IRoutePublisher publisher = new RoutePublisher(routes, new StubContainerProvider(null, null));
            publisher.Publish(new[] { Desc(null, "bar"), Desc(string.Empty, "quux") });

            Assert.That(routes.Count(), Is.EqualTo(2));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SameNameTwiceCausesExplosion() {
            var routes = new RouteCollection();
            routes.MapRoute("foo", "{controller}");

            IRoutePublisher publisher = new RoutePublisher(routes, new StubContainerProvider(null, null));
            publisher.Publish(new[] {Desc("yarg", "bar"), Desc("yarg", "quux")});

            Assert.That(routes.Count(), Is.EqualTo(2));
        }


        [Test]
        public void ExplosionLeavesOriginalRoutesIntact() {
            var routes = new RouteCollection();
            routes.MapRoute("foo", "{controller}");

            IRoutePublisher publisher = new RoutePublisher(routes, new StubContainerProvider(null, null));
            try {
                publisher.Publish(new[] { Desc("yarg", "bar"), Desc("yarg", "quux") });
            }
            catch (ArgumentException) {
                Assert.That(routes.Count(), Is.EqualTo(1));
                Assert.That(routes.OfType<Route>().Single().Url, Is.EqualTo("{controller}"));
            }
        }

        [Test]
        public void RoutesArePaintedWithConainerProviderAsTheyAreApplied() {
            var routes = new RouteCollection();
            routes.MapRoute("foo", "{controller}");

            var containerProvider = new StubContainerProvider(null, null);
            IRoutePublisher publisher = new RoutePublisher(routes, containerProvider);
            publisher.Publish(new[] { Desc("barname", "bar"), Desc("quuxname", "quux") });

            Assert.That(routes.OfType<Route>().Count(), Is.EqualTo(2));
            Assert.That(routes.OfType<Route>().SelectMany(r => r.DataTokens.Values).Count(), Is.EqualTo(2));
            Assert.That(routes.OfType<Route>().SelectMany(r => r.DataTokens.Values), Has.All.SameAs(containerProvider));
        }

        [Test]
        public void WriteBlocksWhileReadIsInEffect() {
            var routes = new RouteCollection();
            routes.MapRoute("foo", "{controller}");

            var containerProvider = new StubContainerProvider(null, null);
            IRoutePublisher publisher = new RoutePublisher(routes, containerProvider);

            var readLock = routes.GetReadLock();

            string where = "init";
            var action = new Action(() => {
                where = "before";
                publisher.Publish(new[] { Desc("barname", "bar"), Desc("quuxname", "quux") });
                where = "after";
            });

            Assert.That(where, Is.EqualTo("init"));
            var async = action.BeginInvoke(null, null);
            Thread.Sleep(75);
            Assert.That(where, Is.EqualTo("before"));
            readLock.Dispose();
            Thread.Sleep(75);
            Assert.That(where, Is.EqualTo("after"));
            action.EndInvoke(async);
        }
    }
}
