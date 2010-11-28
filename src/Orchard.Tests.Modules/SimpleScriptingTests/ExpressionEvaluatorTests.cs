using NUnit.Framework;
using Orchard.Widgets.SimpleScripting;

namespace Orchard.Tests.Modules.SimpleScriptingTests {
    [TestFixture]
    public class ExpressionEvaluatorTests {
        [Test]
        public void EvaluateSimpleConstant() {
            var tree = new ExpressionParser("1*2+3").Parse();

        }
    }
}
