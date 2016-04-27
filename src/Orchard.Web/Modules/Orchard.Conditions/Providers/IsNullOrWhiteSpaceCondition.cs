using System;
using Orchard.Conditions.Services;
using Orchard.Security;

namespace Orchard.Conditions.Providers {
    public class IsNullOrWhiteSpaceCondition : IConditionProvider {

        public IsNullOrWhiteSpaceCondition() {            
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) { 
            if (!String.Equals(evaluationContext.FunctionName, "isNullOrWhiteSpaceCondition", StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            var arg1 = Convert.ToString(evaluationContext.Arguments[0]);

            evaluationContext.Result = string.IsNullOrWhiteSpace(arg1);
        }
    }
}