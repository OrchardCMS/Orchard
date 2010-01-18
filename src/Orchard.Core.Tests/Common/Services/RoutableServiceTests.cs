using Autofac;
using Autofac.Builder;
using NUnit.Framework;
using Orchard.Core.Common.Services;

namespace Orchard.Core.Tests.Common.Services {
    [TestFixture]
    public class RoutableServiceTests {
        #region Setup/Teardown

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.Register<RoutableService>().As<IRoutableService>();
            IContainer container = builder.Build();
            _routableService = container.Resolve<IRoutableService>();
        }

        #endregion

        private IRoutableService _routableService;

        [Test]
        public void BeginningSlashesShouldBeReplacedByADash() {
            Assert.That(_routableService.Slugify("/slug"), Is.EqualTo("-slug"));
            Assert.That(_routableService.Slugify("//slug"), Is.EqualTo("-slug"));
            Assert.That(_routableService.Slugify("//////////////slug"), Is.EqualTo("-slug"));
        }

        [Test]
        public void MultipleSlashesShouldBecomeOne() {
            Assert.That(_routableService.Slugify("/slug//with///lots/of////s/lashes"), Is.EqualTo("-slug/with/lots/of/s/lashes"));
            Assert.That(_routableService.Slugify("slug/with/a/couple//slashes"), Is.EqualTo("slug/with/a/couple/slashes"));
        }

        [Test]
        public void InvalidCharactersShouldBeReplacedByADash() {
            Assert.That(_routableService.Slugify(
                            "Please do not use any of the following characters in your slugs: \":\", \"/\", \"?\", \"#\", \"[\", \"]\", \"@\", \"!\", \"$\", \"&\", \"'\", \"(\", \")\", \"*\", \"+\", \",\", \";\", \"=\""),
                        Is.EqualTo(
                            "Please-do-not-use-any-of-the-following-characters-in-your-slugs-\"-\"-\"/\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\"-\""));
        }

        [Test]
        public void VeryLongStringTruncatedTo1000Chars() {
            var veryVeryLongSlug = "this is a very long slug...";
            for (var i = 0; i < 100; i++)
                veryVeryLongSlug += "aaaaaaaaaa";

            Assert.That(veryVeryLongSlug.Length, Is.AtLeast(1001));
            Assert.That(_routableService.Slugify(veryVeryLongSlug).Length, Is.EqualTo(1000));
        }
    }
}