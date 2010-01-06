using System.Web.Routing;
using NUnit.Framework;
using Orchard.Pages.Services;

namespace Orchard.Tests.Packages.Pages.Services {
    [TestFixture]
    public class SlugConstraintTests {
        [Test]
        public void MatchShouldBeTrueWhenSlugIsInSet() {
            var slugConstraint = new SlugConstraint();

            var before = slugConstraint.Match(null, null, "slug", new RouteValueDictionary {{"slug", "foo"}},
                                 RouteDirection.IncomingRequest);
            Assert.That(before, Is.False);

            slugConstraint.SetCurrentlyPublishedSlugs(new[]{"foo"});
            var after = slugConstraint.Match(null, null, "slug", new RouteValueDictionary { { "slug", "foo" } },
                                 RouteDirection.IncomingRequest);
            Assert.That(after, Is.True);
        }

        [Test]
        public void MatchShouldIgnoreCase() {
            var slugConstraint = new SlugConstraint();
            slugConstraint.SetCurrentlyPublishedSlugs(new[]{"foo", "bAr"});

            var foo = slugConstraint.Match(null, null, "slug", new RouteValueDictionary { { "slug", "FOO" } },
                                 RouteDirection.IncomingRequest);
            Assert.That(foo, Is.True);

            var bar = slugConstraint.Match(null, null, "slug", new RouteValueDictionary { { "slug", "bar" } },
                     RouteDirection.IncomingRequest);
            Assert.That(bar, Is.True);

        }

        [Test]
        public void CollisionsShouldNotThrowExceptions() {
            var slugConstraint = new SlugConstraint();
            slugConstraint.SetCurrentlyPublishedSlugs(new[] { "foo", "FOO" });


        }
    }
}
