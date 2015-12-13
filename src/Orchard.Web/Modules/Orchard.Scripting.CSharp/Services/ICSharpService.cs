using System;

namespace Orchard.Scripting.CSharp.Services {
    public interface ICSharpService : IDependency {
        void SetParameter(string name, object value);
        void SetFunction(string name, Delegate value);
        void Run(string script);
        object Evaluate(string script);
    }
};