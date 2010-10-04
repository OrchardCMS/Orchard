using System;
using Orchard.UI.Widgets;

namespace Orchard.Widgets.RuleEngine {
    public class UrlRuleProvider : IRuleProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public UrlRuleProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public void Process(RuleContext ruleContext) {
            if (!String.Equals(ruleContext.FunctionName, "Url", StringComparison.OrdinalIgnoreCase)) {
                return;
            }

            var context = _workContextAccessor.GetContext();
            ruleContext.Result = context.HttpContext.Request.RawUrl.StartsWith(Convert.ToString(ruleContext.Arguments[0]));
        }
    }
}