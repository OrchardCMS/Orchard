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
        public void IdentitiesCanBeAddedWithNoPriority() {
            var contentIdentity = new ContentIdentity();

            contentIdentity.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1");
            contentIdentity.Add("alias", "some-unique-item-alias/sub-alias");

            Assert.AreEqual("/alias=some-unique-item-alias\\/sub-alias/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentity.ToString());
        }

        [Test]
        public void IdentitiesCanBeAddedWithSamePriority() {
            var contentIdentity = new ContentIdentity();
            contentIdentity.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1", 5);
            contentIdentity.Add("alias", "some-unique-item-alias/sub-alias", 5);

            var contentIdentityNegative = new ContentIdentity();
            contentIdentityNegative.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1", -5);
            contentIdentityNegative.Add("alias", "some-unique-item-alias/sub-alias", -5);

            Assert.AreEqual("/alias=some-unique-item-alias\\/sub-alias/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentity.ToString());
            Assert.AreEqual("/alias=some-unique-item-alias\\/sub-alias/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentityNegative.ToString());
        }

        [Test]
        public void LowestPriorityIdentityIsIgnored() {
            var contentIdentity = new ContentIdentity();
            contentIdentity.Add("alias", "some-unique-item-alias/sub-alias", 0);
            contentIdentity.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1", 5);

            var contentIdentityNegative = new ContentIdentity();
            contentIdentityNegative.Add("alias", "some-unique-item-alias/sub-alias", -5);
            contentIdentityNegative.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1", 0);

            Assert.AreEqual("/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentity.ToString());
            Assert.AreEqual("/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentityNegative.ToString());
        }

        [Test]
        public void HighestPriorityIdentityIsRetained() {
            var contentIdentity = new ContentIdentity();
            contentIdentity.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1", 5);
            contentIdentity.Add("alias", "some-unique-item-alias/sub-alias", 0);

            var contentIdentityNegative = new ContentIdentity();
            contentIdentityNegative.Add("identifier", "CAEEB150-F5E9-481D-9FF9-3053D23329C1", 0);
            contentIdentityNegative.Add("alias", "some-unique-item-alias/sub-alias", -5);

            Assert.AreEqual("/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentity.ToString());
            Assert.AreEqual("/identifier=CAEEB150-F5E9-481D-9FF9-3053D23329C1", contentIdentityNegative.ToString());
        }
    }
}



