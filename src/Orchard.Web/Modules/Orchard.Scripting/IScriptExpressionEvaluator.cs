using System.Collections.Generic;

namespace Orchard.Scripting {
    public interface IScriptExpressionEvaluator : ISingletonDependency {
        object Evaluate(string expression, IEnumerable<IGlobalMethodProvider> providers);
    }
}