using System;
using System.Collections.Generic;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Services;
using Orchard.Widgets.Services;

namespace Orchard.Layouts.Handlers {
    public class ElementRuleCoordinator : ElementEventHandlerBase {
        private readonly IRuleManager _ruleManager;
        private readonly Dictionary<string, bool> _evaluations = new Dictionary<string, bool>();

        public ElementRuleCoordinator(IRuleManager ruleManager) {
            _ruleManager = ruleManager;
        }

        public override void CreatingDisplay(ElementCreatingDisplayShapeContext context) {
            if (context.DisplayType == "Design")
                return;

            if (String.IsNullOrWhiteSpace(context.Element.Rule))
                return;

            context.Cancel = !EvaluateRule(context.Element.Rule);
        }

        private bool EvaluateRule(string rule) {
            if (_evaluations.ContainsKey(rule))
                return _evaluations[rule];

            var result = _ruleManager.Matches(rule);
            _evaluations[rule] = result;
            return result;
        }
    }
}