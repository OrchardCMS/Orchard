namespace Orchard.Scripting {
    public interface IScriptingManager : IDependency {
        dynamic GetVariable(string name);
        void SetVariable(string name, object value);
        dynamic ExecuteExpression(string expression);
        void ExecuteFile(string fileName);
    }
}
