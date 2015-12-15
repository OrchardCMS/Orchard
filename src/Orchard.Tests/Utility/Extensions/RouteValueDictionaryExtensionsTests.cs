using System.Web.Routing;
using NUnit.Framework;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    class RouteValueDictionaryExtensionsTests {
        [Test]
        public void IdenticalRouteValueDictionariesShouldMatch() {
            Assert.IsTrue(new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" } }
                .Match(new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" } }));
        }

        [Test]
        public void CasedRouteValueDictionariesShouldMatch() {
            Assert.IsTrue(new RouteValueDictionary { { "controller", "foo" }, { "action", "BAR" } }
                .Match(new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" } }));
        }
        
        [Test]
        public void RouteValueDictionariesWithDifferentNumbersOfValuesShouldNotMatch() {
            Assert.IsFalse(new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" } }
                .Match(new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" }, { "area", "baz" } }));

        }

        [Test]
        public void RouteValueDictionariesWithDifferentValuesShouldMatch() {
            Assert.IsFalse(new RouteValueDictionary { { "controller", "foo" }, { "action", "bar" } }
                .Match(new RouteValueDictionary { { "controller", "foo" }, { "action", "baz" } }));

        }
    }
}
