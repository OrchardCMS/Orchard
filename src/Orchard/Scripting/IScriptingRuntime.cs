using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public interface IScriptingRuntime : ISingletonDependency {
        ScriptScope CreateScope();
        dynamic ExecuteExpression(string expression, ScriptScope scope);
        void ExecuteFile(string fileName, ScriptScope scope);
    }
}
