using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public class ScriptingManager : IScriptingManager {
        private readonly IScriptingRuntime _scriptingRuntime;
        private ScriptScope _scriptingScope;

        public ScriptingManager(IScriptingRuntime scriptingRuntime) {
            _scriptingRuntime = scriptingRuntime;
            _scriptingScope = _scriptingRuntime.GetRubyEngine().CreateScope();
        }

        #region IScriptingContext Members

        public dynamic GetVariable(string name) {
            return _scriptingScope.GetVariable(name);
        }

        public void SetVariable(string name, object value) {
            _scriptingScope.SetVariable(name, value);
        }

        public dynamic Eval(string expression) {
            var script = _scriptingRuntime.GetRubyEngine().CreateScriptSourceFromString(expression);
            return script.Execute(_scriptingScope);
        }

        public void ExecuteFile(string fileName) {
            _scriptingRuntime.GetRubyEngine().ExecuteFile(fileName, _scriptingScope);
        }

        public void SetScriptingScope(ScriptScope scriptScope) {
            _scriptingScope = scriptScope;
        }

        #endregion
    }
}
