using System;
using Orchard.Conditions.Services;

namespace Orchard.Conditions.Providers {
    public class BuiltinCondition : IConditionProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public BuiltinCondition(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (string.Equals(evaluationContext.FunctionName, "WorkContext", StringComparison.OrdinalIgnoreCase)) {
                evaluationContext.Result = _workContextAccessor.GetContext();
            }
        }
    }
}