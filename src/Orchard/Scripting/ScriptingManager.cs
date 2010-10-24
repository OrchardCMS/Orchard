using System;
using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public class ScriptingManager : IScriptingManager {
        private readonly IScriptingRuntime _scriptingRuntime;
        private Lazy<ScriptScope> _scope;

        public ScriptingManager(IScriptingRuntime scriptingRuntime) {
            _scriptingRuntime = scriptingRuntime;
            _scope = new Lazy<ScriptScope>(()=>_scriptingRuntime.CreateScope());
        }

        public dynamic GetVariable(string name) {
            return _scope.Value.GetVariable(name);
        }

        public void SetVariable(string name, object value) {
            _scope.Value.SetVariable(name, value);
        }

        public dynamic ExecuteExpression(string expression) {
            return _scriptingRuntime.ExecuteExpression(expression, _scope.Value);
        }

        public void ExecuteFile(string fileName) {
            _scriptingRuntime.ExecuteFile(fileName, _scope.Value);
        }

    }
}
