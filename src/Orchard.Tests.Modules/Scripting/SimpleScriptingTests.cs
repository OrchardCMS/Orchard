using System.Linq;
using NUnit.Framework;
using Orchard.Scripting;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Scripting {
    [TestFixture]
    public class SimpleScriptingTests {
        [Test]
        public void EngineThrowsSyntaxErrors() {
            var engine = new ScriptExpressionEvaluator(new StubCacheManager());
            Assert.That(() => engine.Evaluate("true+", Enumerable.Empty<IGlobalMethodProvider>()), Throws.Exception);
        }
        [Test]
        public void EngineUnderstandsPrimitiveValues() {
            var engine = new ScriptExpressionEvaluator(new StubCacheManager());
            Assert.That(engine.Evaluate("true", Enumerable.Empty<IGlobalMethodProvider>()), Is.True);
        }
        [Test]
        public void EngineUnderstandsPrimitiveValues2() {
            var engine = new ScriptExpressionEvaluator(new StubCacheManager());
            Assert.That(engine.Evaluate("true and true", Enumerable.Empty<IGlobalMethodProvider>()), Is.True);
        }
    }
}
