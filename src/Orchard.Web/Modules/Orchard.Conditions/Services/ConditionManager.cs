using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Scripting;

namespace Orchard.Conditions.Services {
    public class ConditionManager : IConditionManager {
        private readonly IConditionProvider _conditions;
        private readonly IEnumerable<IScriptExpressionEvaluator> _evaluators;

        public ConditionManager(IConditionProvider conditions, IEnumerable<IScriptExpressionEvaluator> evaluators) {
            _conditions = conditions;
            _evaluators = evaluators;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public bool Matches(string expression) {
            var evaluator = _evaluators.FirstOrDefault();
            if (evaluator == null) {
                throw new OrchardException(T("There are currently no scripting engines enabled"));
            }

            var result = evaluator.Evaluate(expression, new List<IGlobalMethodProvider> { new GlobalMethodProvider(this) });
            if (!(result is bool)) {
                throw new OrchardException(T("Expression is not a boolean value"));
            }
            return (bool)result;
        }

        private class GlobalMethodProvider : IGlobalMethodProvider {
            private readonly ConditionManager _conditionManager;

            public GlobalMethodProvider(ConditionManager conditionManager) {
                _conditionManager = conditionManager;
            }

            public void Process(GlobalMethodContext context) {
                var ruleContext = new ConditionEvaluationContext {
                    FunctionName = context.FunctionName,
                    Arguments = context.Arguments.ToArray(),
                    Result = context.Result
                };

                _conditionManager._conditions.Evaluate(ruleContext);

                context.Result = ruleContext.Result;
            }
        }
    }
}