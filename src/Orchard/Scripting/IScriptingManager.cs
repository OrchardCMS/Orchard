using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public interface IScriptingManager : IDependency {
        dynamic GetVariable(string name);
        void SetVariable(string name, object value);
        dynamic Eval(string expression);
        void ExecuteFile(string fileName);
        void SetScriptingScope(ScriptScope scriptScope);
    }
}
