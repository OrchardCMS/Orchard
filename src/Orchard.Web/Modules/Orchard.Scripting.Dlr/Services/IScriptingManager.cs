using System;
using Microsoft.Scripting.Hosting;

namespace Orchard.Scripting.Dlr.Services {
    public interface IScriptingManager : IDependency {
        dynamic GetVariable(string name);
        void SetVariable(string name, object value);
        dynamic ExecuteExpression(string expression);
        void ExecuteFile(string fileName);
        dynamic ExecuteOperation(Func<ObjectOperations, object> invoke);
    }
}