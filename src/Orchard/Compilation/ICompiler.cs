using System.Collections.Generic;

namespace Orchard.Compilation {
    public interface ICompiler{
        object Compile(string code, IDictionary<string, object> parameters);
        T Compile<T>(string code, IDictionary<string, object> parameters);
    }
}