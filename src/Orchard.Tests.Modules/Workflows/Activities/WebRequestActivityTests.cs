using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Orchard.Workflows.Activities;

namespace Orchard.Tests.Modules.Workflows.Activities {
    [TestFixture]
    public class WebRequestActivityTests {

        private readonly MethodInfo _parseKeyValueString = typeof(WebRequestActivity).GetMethod("ParseKeyValueString", BindingFlags.NonPublic | BindingFlags.Static);

        private IEnumerable<KeyValuePair<string, string>> ParseKeyValueString(string text) {
            return (IEnumerable<KeyValuePair<string, string>>)_parseKeyValueString.Invoke(null, new object[] { text });
        }

        [Test]
        public void ParseKeyValueStringShouldRecognizeNewLinePatterns() {
            Assert.That(ParseKeyValueString("a=b\nc=d\r\ne=f"), Is.EquivalentTo( new Dictionary<string, string> { {"a", "b"}, {"c", "d"}, {"e","f"}}));
        }

        [Test]
        public void ParseKeyValueStringShouldIgnoreComments() {
            Assert.That(ParseKeyValueString("a=b\n#c=d\ne=f"), Is.EquivalentTo(new Dictionary<string, string> { { "a", "b" }, { "e", "f" } }));
        }

        [Test]
        public void ParseKeyValueStringShouldIgnoreEmptyLines() {
            Assert.That(ParseKeyValueString("a=b\n\n\n\n\ne=f"), Is.EquivalentTo(new Dictionary<string, string> { { "a", "b" }, { "e", "f" } }));
        }

        [Test]
        public void ParseKeyValueStringShouldIgnoreMalformedLines() {
            Assert.That(ParseKeyValueString("a=b\nc:d\ne=f"), Is.EquivalentTo(new Dictionary<string, string> { { "a", "b" }, { "e", "f" } }));
        }

    }
}
