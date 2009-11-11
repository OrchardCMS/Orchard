using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Orchard.UI.Navigation;

namespace Orchard.Tests.UI.Navigation {
    [TestFixture]
    public class NavigationManagerTests {
        [Test]
        public void EmptyMenuIfNameDoesntMatch() {
            var manager = new NavigationManager(new[] { new StubProvider() });

            var menuItems = manager.BuildMenu("primary");
            Assert.That(menuItems.Count(), Is.EqualTo(0));
        }

        [Test]
        public void NavigationManagerShouldUseProvidersToBuildNamedMenu() {
            var manager = new NavigationManager(new[] { new StubProvider() });

            var menuItems = manager.BuildMenu("admin");
            Assert.That(menuItems.Count(), Is.EqualTo(2));
            Assert.That(menuItems.First(), Has.Property("Text").EqualTo("Foo"));
            Assert.That(menuItems.Last(), Has.Property("Text").EqualTo("Bar"));
            Assert.That(menuItems.Last().Contained.Count(), Is.EqualTo(1));
            Assert.That(menuItems.Last().Contained.Single().Text, Is.EqualTo("Frap"));
        }

        [Test]
        public void NavigationManagerShouldMergeAndOrderNavigation() {
            var manager = new NavigationManager(new INavigationProvider[] { new StubProvider(), new Stub2Provider() });

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

            Assert.That(item2.Contained.Count(), Is.EqualTo(2));
            var subitem1 = item2.Contained.First();
            var subitem2 = item2.Contained.Last();
            Assert.That(subitem1.Text, Is.EqualTo("Quad"));
            Assert.That(subitem1.Position, Is.EqualTo("1.a"));
            Assert.That(subitem2.Text, Is.EqualTo("Frap"));
            Assert.That(subitem2.Position, Is.EqualTo("1.b"));

        }

        public class StubProvider : INavigationProvider {
            public string MenuName { get { return "admin"; } }

            public void GetNavigation(NavigationBuilder builder) {
                builder
                    .Add("Foo", "1.0")
                    .Add("Bar", "2.0", x => x.Add("Frap", "1.b"));
            }
        }

        public class Stub2Provider : INavigationProvider {
            public string MenuName { get { return "admin"; } }

            public void GetNavigation(NavigationBuilder builder) {
                builder
                    .Add("Frap", "3.0")
                    .Add("Bar", "4.0", x => x.Add("Quad", "1.a"));
            }
        }
    }

}
