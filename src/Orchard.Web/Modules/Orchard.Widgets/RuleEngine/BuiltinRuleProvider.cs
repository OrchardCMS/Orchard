using System;
using Orchard.Widgets.Services;

namespace Orchard.Widgets.RuleEngine {
    public class BuiltinRuleProvider : IRuleProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public BuiltinRuleProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Process(RuleContext ruleContext) {
            if (string.Equals(ruleContext.FunctionName, "WorkContext", StringComparison.OrdinalIgnoreCase)) {
                ruleContext.Result = _workContextAccessor.GetContext();
            }
        }
    }
}