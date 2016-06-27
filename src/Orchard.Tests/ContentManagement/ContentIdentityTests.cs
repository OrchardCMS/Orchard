using System;
using NUnit.Framework;
using Orchard.ContentManagement;

namespace Orchard.Tests.ContentManagement {
    [TestFixture]
    public class ContentIdentityTests {
        [Test]
        public void ContentIdentityParsesIdentities() {
            var identity1 = new ContentIdentity("/foo=bar");
            Assert.That(identity1.Get("foo"), Is.EqualTo("bar"));

            var identity2 = new ContentIdentity("/foo=");
            Assert.That(identity2.Get("foo"), Is.EqualTo(String.Empty));

            var identity3 = new ContentIdentity("foo");
            Assert.That(identity3.Get("foo"), Is.Null);
        }

        [Test]
        public void ContentIdentitiesAreEncodedWhenOutput() {
            var identity1 = new ContentIdentity("/foo=bar");
            Assert.That(identity1.ToString(), Is.EqualTo("/foo=bar"));

            var identity2 = new ContentIdentity(@"/foo=bar/abaz=quux\/fr\\ed=foo/yarg=yiu=foo");
            Assert.That(identity2.Get("foo"), Is.EqualTo("bar"));
            Assert.That(identity2.Get("abaz"), Is.EqualTo(@"quux/fr\ed=foo"));
            Assert.That(identity2.Get("yarg"), Is.EqualTo("yiu=foo"));
            Assert.That(identity2.ToString(), Is.EqualTo(@"/abaz=quux\/fr\\ed=foo/foo=bar/yarg=yiu=foo"));
        }

        [Test]
        public void ContentIdentitiesWithKeysAddedInDifferentOrderAreEqual() {
            var comparer = new ContentIdentity.ContentIdentityEqualityComparer();

            var identity1 = new ContentIdentity("/foo=bar");
            Assert.That(comparer.Equals(identity1, new ContentIdentity(identity1.ToString())));

            var identity2 = new ContentIdentity(@"/foo=bar/abaz=quux\/fr\\ed=foo/yarg=yiu=foo");
            Assert.That(comparer.Equals(identity2, new ContentIdentity(identity2.ToString())));
        }

        [Test]
        public void ContentIdentityCanSeePartialMatchesAreEquivalent() {
            var identity1 = new ContentIdentity("/bar=baz/a=b");
            var identity2 = new ContentIdentity(@"/foo=bar/bar=baz/glop=glop");
            Assert.That(ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(identity1, identity2));
            Assert.That(ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(identity2, identity1));
        }

        [Test]
        public void ContentIdentityCanSeeFullMatchesAreEquivalent() {
            var identity1 = new ContentIdentity(@"/foo=bar/bar=baz/glop=glop");
            var identity2 = new ContentIdentity(@"/foo=bar/bar=baz/glop=glop");
            Assert.That(ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(identity1, identity2));
            Assert.That(ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(identity2, identity1));
        }

        [Test]
        public void ContentIdentityCanSeeNonMatchesAreNotEquivalent() {
            var identity1 = new ContentIdentity(@"/a=b/foo=baz");
            var identity2 = new ContentIdentity(@"/foo=bar/bar=baz/glop=glop");
            Assert.IsFalse(ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(identity1, identity2));
            Assert.IsFalse(ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(identity2, identity1));
        }
    }
}



