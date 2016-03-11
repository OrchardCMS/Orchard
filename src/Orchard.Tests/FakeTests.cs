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
        [ExpectedException(typeof (ApplicationException), ExpectedMessage = "Boom")]
        public void ExceptionsCanBeVerified() {
            throw new ApplicationException("Boom");
        }

        [Test]
        public void TestShouldRunFromResharper() {
            Assert.That(_x, Is.EqualTo(5));
        }
    }
}