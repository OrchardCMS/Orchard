using System;
using System.Collections.Generic;
using Orchard.Conditions.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Handlers {
    public class ElementRuleCoordinator : ElementEventHandlerBase {
        private readonly IConditionManager _conditionManager;
        private readonly Dictionary<string, bool> _evaluations = new Dictionary<string, bool>();

        public ElementRuleCoordinator(IConditionManager conditionManager) {
            _conditionManager = conditionManager;
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

            var result = _conditionManager.Matches(rule);
            _evaluations[rule] = result;
            return result;
        }
    }
}