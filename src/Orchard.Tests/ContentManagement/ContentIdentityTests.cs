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
            Assert.That(identity2.ToString(), Is.EqualTo(@"/foo=bar/abaz=quux\/fr\\ed=foo/yarg=yiu=foo"));
        }
    }
}



