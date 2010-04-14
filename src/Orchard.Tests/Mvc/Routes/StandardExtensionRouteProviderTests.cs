using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.Mvc.Routes;
using Orchard.Extensions;

namespace Orchard.Tests.Mvc.Routes {
    [TestFixture]
    public class StandardExtensionRouteProviderTests {
        [Test]
        public void ExtensionDisplayNameShouldBeUsedInBothStandardRoutes() {
            var stubManager = new StubExtensionManager();
            var routeProvider = new StandardExtensionRouteProvider(stubManager);

            var routes = new List<RouteDescriptor>();
            routeProvider.GetRoutes(routes);

            Assert.That(routes, Has.Count.EqualTo(4));
            var fooAdmin = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Admin/Foo/{action}/{id}");
            var fooRoute = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Foo/{controller}/{action}/{id}");
            var barAdmin = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Admin/Bar/{action}/{id}");
            var barRoute = routes.Select(x => x.Route).OfType<Route>()
                .Single(x => x.Url == "Bar/{controller}/{action}/{id}");

            Assert.That(fooAdmin.DataTokens["area"], Is.EqualTo("Long.Name.Foo"));
            Assert.That(fooRoute.DataTokens["area"], Is.EqualTo("Long.Name.Foo"));
            Assert.That(barAdmin.DataTokens["area"], Is.EqualTo("Long.Name.Bar"));
            Assert.That(barRoute.DataTokens["area"], Is.EqualTo("Long.Name.Bar"));
        }

        public class StubExtensionManager : IExtensionManager {
            public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
                throw new NotImplementedException();
            }

            public IEnumerable<ExtensionEntry> ActiveExtensions() {
                yield return new ExtensionEntry {
                    Descriptor = new ExtensionDescriptor {
                        Name = "Long.Name.Foo",
                        DisplayName = "Foo",
                    }
                };
                yield return new ExtensionEntry {
                    Descriptor = new ExtensionDescriptor {
                        Name = "Long.Name.Bar",
                        DisplayName = "Bar",
                    }
                };
            }

            public ShellTopology_Obsolete GetExtensionsTopology() {
                throw new NotImplementedException();
            }

            public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
                throw new NotImplementedException();
            }

            public void UninstallExtension(string extensionType, string extensionName) {
                throw new NotImplementedException();
            }
        }
    }
}
