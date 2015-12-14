using System;
using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting.Dlr.Services {
    public class ScriptingManager : IScriptingManager {
        private readonly IScriptingRuntime _scriptingRuntime;
        private readonly Lazy<ScriptScope> _scope;
        private readonly Lazy<ObjectOperations> _operations;

        public ScriptingManager(IScriptingRuntime scriptingRuntime) {
            _scriptingRuntime = scriptingRuntime;
            _scope = new Lazy<ScriptScope>(()=>_scriptingRuntime.CreateScope());
            _operations = new Lazy<ObjectOperations>(()=>_scope.Value.Engine.Operations);
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

        public dynamic ExecuteOperation(Func<ObjectOperations, object> invoke) {
            return invoke(_operations.Value);
        }

        public void ExecuteFile(string fileName) {
            _scriptingRuntime.ExecuteFile(fileName, _scope.Value);
        }
    }
}