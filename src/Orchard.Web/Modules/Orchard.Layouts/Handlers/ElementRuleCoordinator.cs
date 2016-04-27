using System;
using System.Collections.Generic;
using Orchard.Conditions.Services;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Services;
using Orchard.Tokens;

namespace Orchard.Layouts.Handlers {
    public class ElementRuleCoordinator : ElementEventHandlerBase {
        private readonly IConditionManager _conditionManager;
        private readonly Dictionary<string, bool> _evaluations = new Dictionary<string, bool>();
        private readonly ITokenizer _tokenizer;

        public ElementRuleCoordinator(IConditionManager conditionManager, ITokenizer tokenizer) {
            _conditionManager = conditionManager;
            _tokenizer = tokenizer;
        }

        public override void CreatingDisplay(ElementCreatingDisplayShapeContext context) {
            if (context.DisplayType == "Design")
                return;

            if (String.IsNullOrWhiteSpace(context.Element.Rule))
                return;

            context.Cancel = !EvaluateRule(context.Element.Rule, new { Element = context.Element });
        }

        private bool EvaluateRule(string rule, object tokenData) {
            if (_evaluations.ContainsKey(rule))
                return _evaluations[rule];

            rule = _tokenizer.Replace(rule, tokenData);
            var result = _conditionManager.Matches(rule);
            _evaluations[rule] = result;
            return result;
        }
    }
}