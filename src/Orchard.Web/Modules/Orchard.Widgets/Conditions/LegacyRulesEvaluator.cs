using System;
using Orchard.Conditions.Services;
using Orchard.Localization;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.Conditions {
    /// <summary>
    /// Evaluates rules implementing the deprecated IRuleProvider (third party modules).
    /// </summary>
    [Obsolete("This is here for backwards compatibility during the deprecation period.")]
    public class LegacyRulesEvaluator : IConditionProvider {
        private readonly IRuleProvider _ruleProviders;

        public LegacyRulesEvaluator(IRuleProvider ruleProviders)
        {
            _ruleProviders = ruleProviders;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            var ruleContext = new RuleContext {
                FunctionName = evaluationContext.FunctionName,
                Arguments = evaluationContext.Arguments,
                Result = evaluationContext.Result
            };

            _ruleProviders.Process(ruleContext);
            evaluationContext.Result = ruleContext.Result;
        }
    }
}