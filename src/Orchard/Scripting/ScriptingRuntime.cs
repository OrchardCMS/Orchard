using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public class ScriptingRuntime : IScriptingRuntime {
        private readonly ScriptRuntime _scriptingRuntime;

        public ScriptingRuntime() {
            var setup = new ScriptRuntimeSetup();
            setup.LanguageSetups.Add(Ruby.CreateRubySetup());
            _scriptingRuntime = new ScriptRuntime(setup);
        }

        public ScriptEngine GetRubyEngine() {
            return _scriptingRuntime.GetEngine("ruby");
        }
    }
}
