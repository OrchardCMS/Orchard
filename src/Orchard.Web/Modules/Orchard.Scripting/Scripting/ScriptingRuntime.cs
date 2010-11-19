using IronRuby;
using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public class ScriptingRuntime : IScriptingRuntime {
        private readonly LanguageSetup _defaultLanguageSetup;
        private readonly ScriptRuntime _scriptingRuntime;

        public ScriptingRuntime() {
            _defaultLanguageSetup = Ruby.CreateRubySetup();

            var setup = new ScriptRuntimeSetup();
            setup.LanguageSetups.Add(_defaultLanguageSetup);
            _scriptingRuntime = new ScriptRuntime(setup);
        }

        ScriptEngine GetDefaultEngine() {
            return _scriptingRuntime.GetEngineByTypeName(_defaultLanguageSetup.TypeName);
        }

        public ScriptScope CreateScope() {
            return _scriptingRuntime.CreateScope();
        }

        public dynamic ExecuteExpression(string expression, ScriptScope scope) {
            var engine = GetDefaultEngine();
            var source = engine.CreateScriptSourceFromString(expression);
            return source.Execute(scope);
        }

        public void ExecuteFile(string fileName, ScriptScope scope) {
            var engine = GetDefaultEngine();
            engine.ExecuteFile(fileName, scope);
        }
    }
}
