using System;
using Orchard.Conditions.Services;
using Orchard.Security;

namespace Orchard.Conditions.Providers {
    public class CompareCondition : IConditionProvider {

        public CompareCondition() {            
        }

        public void Evaluate(ConditionEvaluationContext evaluationContext) { 
            if (!String.Equals(evaluationContext.FunctionName, "compare", StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            var arg1 = Convert.ToString(evaluationContext.Arguments[0]);
            var arg2 = Convert.ToString(evaluationContext.Arguments[1]);

            evaluationContext.Result = (arg1.CompareTo(arg2)==0);
        }
    }
}