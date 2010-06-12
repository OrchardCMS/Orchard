using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.Blueprint.Models;
using Orchard.Mvc.Routes;

namespace Orchard.Tests.Mvc.Routes {
    [TestFixture]
    public class StandardExtensionRouteProviderTests {
        [Test]
        public void ExtensionDisplayNameShouldBeUsedInBothStandardRoutes() {
            var blueprint = new ShellBlueprint {
                Controllers = new[] {
                    new ControllerBlueprint {
                        AreaName ="Long.Name.Foo",
                        Feature =new Feature {
                            Descriptor=new FeatureDescriptor {
                                Extension=new ExtensionDescriptor {
                                    DisplayName="Foo"
                                }
                            }
                        }
                    },
                    new ControllerBlueprint {
                        AreaName ="Long.Name.Bar",
                        Feature =new Feature {
                            Descriptor=new FeatureDescriptor {
                                Extension=new ExtensionDescriptor {
                                    DisplayName="Bar"
                                }
                            }
                        }
                    }
                }
            };
            var routeProvider = new StandardExtensionRouteProvider(blueprint);

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

            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> features) {
                throw new NotImplementedException();
            }

            public IEnumerable<ExtensionEntry> ActiveExtensions_Obsolete() {
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

            public void InstallExtension(string extensionType, HttpPostedFileBase extensionBundle) {
                throw new NotImplementedException();
            }

            public void UninstallExtension(string extensionType, string extensionName) {
                throw new NotImplementedException();
            }
        }
    }
}
