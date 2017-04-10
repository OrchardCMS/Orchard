using System.Web.Routing;
using NUnit.Framework;
using Orchard.Tests.Stubs;
using Orchard.UI.Admin;
using Orchard.Themes.Services;
using Autofac;
using Moq;
using Orchard.Tests.Utility;
namespace Orchard.Tests.Modules.Themes.Services
{
    [TestFixture]
    public class AdminThemeSelectorTests : DatabaseEnabledTestsBase
    {
        private IAdminThemeService _adminThemeService;
        public override void Register(ContainerBuilder builder)
        { 

            builder.RegisterAutoMocking(MockBehavior.Loose);
        
            builder.RegisterType<AdminThemeService>().As<IAdminThemeService>();
           
        }
        public override void Init()
        {
            base.Init();

            _adminThemeService = _container.Resolve<IAdminThemeService>();
 
        }
        [Test]
        public void IsAppliedShouldBeFalseByDefault() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            var isApplied = AdminFilter.IsApplied(context);
            Assert.That(isApplied, Is.False);
        }

        [Test]
        public void IsAppliedShouldBeTrueAfterBeingApplied() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            Assert.That(AdminFilter.IsApplied(context), Is.False);
            AdminFilter.Apply(context);
            Assert.That(AdminFilter.IsApplied(context), Is.True);
        }


        [Test]
        public void IsAppliedIsFalseOnNewContext() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());
            Assert.That(AdminFilter.IsApplied(context), Is.False);
            AdminFilter.Apply(context);
            Assert.That(AdminFilter.IsApplied(context), Is.True);
            context = new RequestContext(new StubHttpContext(), new RouteData());
            Assert.That(AdminFilter.IsApplied(context), Is.False);
        }

        [Test]
        public void ThemeResultShouldBeNullNormally() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());

            var selector = new AdminThemeSelector(_adminThemeService);
            var result = selector.GetTheme(context);
            Assert.That(result, Is.Null);
        }


        [Test]
        public void ThemeResultShouldBeTheAdminAt100AfterBeingSet() {
            var context = new RequestContext(new StubHttpContext(), new RouteData());

            AdminFilter.Apply(context);

            var selector = new AdminThemeSelector(_adminThemeService);
            var result = selector.GetTheme(context);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ThemeName, Is.EqualTo("TheAdmin"));
            Assert.That(result.Priority, Is.EqualTo(100));
        }
    }
}
