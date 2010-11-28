using NUnit.Framework;
using Orchard.Widgets.SimpleScripting;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Tests.Modules.SimpleScriptingTests {
    [TestFixture]
    public class EvaluatorTests {
        [Test]
        public void EvaluateSimpleConstant() {
            var tree = new Parser("1*2+3").Parse();

        }
    }
}
