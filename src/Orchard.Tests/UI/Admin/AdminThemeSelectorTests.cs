using System.Web.Routing;
using NUnit.Framework;
using Orchard.Tests.Stubs;
using Orchard.UI.Admin;

namespace Orchard.Tests.UI.Admin {
    [TestFixture]
    public class AdminThemeSelectorTests {
        [Test]
        public void IsAppliedShouldBeFalseByDefault() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            var isApplied = AdminThemeSelector.IsApplied(context);
            Assert.That(isApplied, Is.False);
        }

        [Test]
        public void IsAppliedShouldBeTrueAfterBeingApplied() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            Assert.That(AdminThemeSelector.IsApplied(context), Is.False);
            AdminThemeSelector.Apply(context);
            Assert.That(AdminThemeSelector.IsApplied(context), Is.True);
        }


        [Test]
        public void IsAppliedIsFalseOnNewContext() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            Assert.That(AdminThemeSelector.IsApplied(context), Is.False);
            AdminThemeSelector.Apply(context);
            Assert.That(AdminThemeSelector.IsApplied(context), Is.True);
            context = new RequestContext(new StubHttpContext(), new RouteData());
            Assert.That(AdminThemeSelector.IsApplied(context), Is.False);
        }

        [Test]
        public void ThemeResultShouldBeNullNormally() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());

            var selector = new AdminThemeSelector();
            var result = selector.GetTheme(context);
            Assert.That(result, Is.Null);
        }


        [Test]
        public void ThemeResultShouldBeTheAdminAt100AfterBeingSet() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            
            AdminThemeSelector.Apply(context);

            var selector = new AdminThemeSelector();
            var result = selector.GetTheme(context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThemeName, Is.EqualTo("TheAdmin"));
            Assert.That(result.Priority, Is.EqualTo(100));
        }
    }
}
