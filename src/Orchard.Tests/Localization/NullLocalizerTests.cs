using NUnit.Framework;
using Orchard.Localization;

namespace Orchard.Tests.Localization {
    [TestFixture]
    public class NullLocalizerTests {
        [Test]
        public void StringsShouldPassThrough() {
            var result = NullLocalizer.Instance("hello world");
            Assert.That(result.ToString(), Is.EqualTo("hello world"));
        }

        [Test]
        public void StringsShouldFormatIfArgumentsArePassedIn() {
            var result = NullLocalizer.Instance("hello {0} world", "!");
            Assert.That(result.ToString(), Is.EqualTo("hello ! world"));
        }

        [Test]
        public void StringsShouldNotFormatWithoutAnyArguments() {
            var result = NullLocalizer.Instance("hello {0} world");
            Assert.That(result.ToString(), Is.EqualTo("hello {0} world"));
        }
    }
}
