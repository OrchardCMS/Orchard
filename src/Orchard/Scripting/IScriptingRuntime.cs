using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting {
    public interface IScriptingRuntime : ISingletonDependency {
        ScriptEngine GetRubyEngine();
    }
}
