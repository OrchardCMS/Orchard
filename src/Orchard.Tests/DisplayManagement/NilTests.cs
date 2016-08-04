using NUnit.Framework;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class NilTests {

        [Test]
        public void NilShouldEqualToNull() {
            var nil = Nil.Instance;

            Assert.That(nil == null, Is.True);
            Assert.That(nil != null, Is.False);

            Assert.That(nil == Nil.Instance, Is.True);
            Assert.That(nil != Nil.Instance, Is.False);
        }

        [Test]
        public void NilShouldBeRecursive() {
            dynamic nil = Nil.Instance;

            Assert.That(nil == null, Is.True);
            Assert.That(nil.Foo == null, Is.True);
            Assert.That(nil.Foo.Bar == null, Is.True);
        }


        [Test]
        public void CallingToStringOnNilShouldReturnEmpty() {
            var nil = Nil.Instance;
            Assert.That(nil.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void CallingToStringOnDynamicNilShouldReturnEmpty() {
            dynamic nil = Nil.Instance;
            Assert.That(nil.Foo.Bar.ToString(), Is.EqualTo(""));
        }

        [Test]
        public void ConvertingToStringShouldReturnNullString() {
            dynamic nil = Nil.Instance;
            Assert.That((string)nil == null, Is.True);
        }
    }
}
