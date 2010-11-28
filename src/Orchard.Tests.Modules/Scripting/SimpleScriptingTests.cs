using System.Linq;
using NUnit.Framework;
using Orchard.Tests.Stubs;
using Orchard.Widgets.Services;
using Orchard.Widgets.SimpleScripting;

namespace Orchard.Tests.Modules.Scripting {
    [TestFixture]
    public class SimpleScriptingTests {
        [Test]
        public void EngineThrowsSyntaxErrors() {
            var engine = new ScriptingEngine(Enumerable.Empty<IRuleProvider>(), new StubCacheManager());
            Assert.That(() => engine.Matches("true+"), Throws.Exception);
        }
        [Test]
        public void EngineThrowsEvalErrors() {
            var engine = new ScriptingEngine(Enumerable.Empty<IRuleProvider>(), new StubCacheManager());
            Assert.That(() => engine.Matches("1 + 1"), Throws.Exception);
        }
        [Test]
        public void EngineUnderstandsPrimitiveValues() {
            var engine = new ScriptingEngine(Enumerable.Empty<IRuleProvider>(), new StubCacheManager());
            Assert.That(engine.Matches("true"), Is.True);
        }
        [Test]
        public void EngineUnderstandsPrimitiveValues2() {
            var engine = new ScriptingEngine(Enumerable.Empty<IRuleProvider>(), new StubCacheManager());
            Assert.That(engine.Matches("true and true"), Is.True);
        }
    }
}
