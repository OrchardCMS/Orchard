using System;
using NUnit.Framework;

namespace Orchard.Tests {
    [TestFixture]
    public class FakeTests {
        #region Setup/Teardown

        [SetUp]
        public void Init() {
            _x = 5;
        }

        #endregion

        private int _x;

        [Test]
        public void ExceptionsCanBeVerified() {
            Assert.Throws<ApplicationException>(() => throw new ApplicationException("Boom"), "Boom");
        }

        [Test]
        public void TestShouldRunFromResharper() {
            Assert.That(_x, Is.EqualTo(5));
        }
    }
}