using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting.Dlr.Services {
    public interface IScriptingRuntime : ISingletonDependency {
        ScriptScope CreateScope();
        dynamic ExecuteExpression(string expression, ScriptScope scope);
        void ExecuteFile(string fileName, ScriptScope scope);
    }
}