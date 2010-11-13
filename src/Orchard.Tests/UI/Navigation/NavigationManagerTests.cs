using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Settings;
using Orchard.Tests.Stubs;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class NavigationManagerTests {
        [Test]
        public void EmptyMenuIfNameDoesntMatch() {
            var manager = new NavigationManager(new[] { new StubProvider() }, new StubAuth(), new UrlHelper(new RequestContext(new StubHttpContext("~/"), new RouteData())), new StubOrchardServices());

            var menuItems = manager.BuildMenu("primary");
            Assert.That(menuItems.Count(), Is.EqualTo(0));
        }

        public class StubAuth : IAuthorizationService {
            public void CheckAccess(Permission permission, IUser user, IContent content) {
            }

            public bool TryCheckAccess(Permission permission, IUser user, IContent content) {
                return true;
            }
        }

        [Test]
        public void NavigationManagerShouldUseProvidersToBuildNamedMenu() {
            var manager = new NavigationManager(new[] { new StubProvider() }, new StubAuth(), new UrlHelper(new RequestContext(new StubHttpContext("~/"), new RouteData())), new StubOrchardServices());

            var menuItems = manager.BuildMenu("admin");
            Assert.That(menuItems.Count(), Is.EqualTo(2));
            Assert.That(menuItems.First(), Has.Property("Text").EqualTo("Foo"));
            Assert.That(menuItems.Last(), Has.Property("Text").EqualTo("Bar"));
            Assert.That(menuItems.Last().Items.Count(), Is.EqualTo(1));
            Assert.That(menuItems.Last().Items.Single().Text, Is.EqualTo("Frap"));
        }

        [Test]
        public void NavigationManagerShouldMergeAndOrderNavigation() {
            var manager = new NavigationManager(new INavigationProvider[] { new StubProvider(), new Stub2Provider() }, new StubAuth(), new UrlHelper(new RequestContext(new StubHttpContext("~/"), new RouteData())), new StubOrchardServices());

            var menuItems = manager.BuildMenu("admin");
            Assert.That(menuItems.Count(), Is.EqualTo(3));

            var item1 = menuItems.First();
            var item2 = menuItems.Skip(1).First();
            var item3 = menuItems.Skip(2).First();

            Assert.That(item1.Text, Is.EqualTo("Foo"));
            Assert.That(item1.Position, Is.EqualTo("1.0"));
            Assert.That(item2.Text, Is.EqualTo("Bar"));
            Assert.That(item2.Position, Is.EqualTo("2.0"));
            Assert.That(item3.Text, Is.EqualTo("Frap"));
            Assert.That(item3.Position, Is.EqualTo("3.0"));

            Assert.That(item2.Items.Count(), Is.EqualTo(2));
            var subitem1 = item2.Items.First();
            var subitem2 = item2.Items.Last();
            Assert.That(subitem1.Text, Is.EqualTo("Quad"));
            Assert.That(subitem1.Position, Is.EqualTo("1.a"));
            Assert.That(subitem2.Text, Is.EqualTo("Frap"));
            Assert.That(subitem2.Position, Is.EqualTo("1.b"));
        }

        public class StubProvider : INavigationProvider {
            public string MenuName { get { return "admin"; } }

            public void GetNavigation(NavigationBuilder builder) {
                var T = NullLocalizer.Instance;
                builder
                    .Add(T("Foo"), "1.0", x => x.Action("foo"))
                    .Add(T("Bar"), "2.0", x => x.Add(T("Frap"), "1.b"));
            }
        }

        public class Stub2Provider : INavigationProvider {
            public string MenuName { get { return "admin"; } }

            public void GetNavigation(NavigationBuilder builder) {
                var T = NullLocalizer.Instance;
                builder
                    .Add(T("Frap"), "3.0", x => x.Action("foo"))
                    .Add(T("Bar"), "4.0", x => x.Add(T("Quad"), "1.a"));
            }
        }
    }

    public class StubOrchardServices : IOrchardServices {
        private readonly ILifetimeScope _lifetimeScope;

        public StubOrchardServices() {}

        public StubOrchardServices(ILifetimeScope lifetimeScope) {
            _lifetimeScope = lifetimeScope;
        }

        public IContentManager ContentManager {
            get { throw new NotImplementedException(); }
        }

        public ITransactionManager TransactionManager {
            get { throw new NotImplementedException(); }
        }

        public IAuthorizer Authorizer {
            get { throw new NotImplementedException(); }
        }

        public INotifier Notifier {
            get { throw new NotImplementedException(); }
        }

        public dynamic New {
            get { throw new NotImplementedException(); }
        }

        public WorkContext WorkContext {
            get { return new StubWorkContextAccessor(_lifetimeScope).GetContext(); }
        }
    }
}
