using System;
using NUnit.Framework;
using Orchard.Caching.Services;

namespace Orchard.Tests.Modules.Email {
    [TestFixture]
    public class CachedTests {
        [Test]
        public void ShouldExplicitlyConvertToValueTypeT() {
            Cached<int> cached = 10;
            var raw = (int)cached;

            Assert.That(raw, Is.EqualTo(10));
        }

        [Test]
        public void ShouldConvertFromValueTypeT() {
            Cached<int> cached1 = 10;
            var cached2 = (Cached<int>)10;

            Assert.That(cached1.Value, Is.EqualTo(10));
            Assert.That(cached2.Value, Is.EqualTo(10));
        }

        [Test]
        public void ShouldImplicitlyConvertFromNull()
        {
            object x = null;
            Cached<object> cached = x;
            Assert.That(cached.HasValue, Is.EqualTo(false));
        }

        [Test]
        public void ShouldCompareWithObjectsOfTypeT() {
            int raw1 = 10;
            int raw2 = 20;
            Cached<int> cached = 10;

            Assert.That(cached == raw1, Is.EqualTo(true));
            Assert.That(raw1 == cached, Is.EqualTo(true));

            Assert.That(cached != raw2, Is.EqualTo(true));
            Assert.That(raw2 != cached, Is.EqualTo(true));
        }
    }

}