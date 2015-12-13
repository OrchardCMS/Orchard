using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;

namespace Orchard.Scripting.Rules {
    public interface IActionProvider : IEventHandler {
        void Describe(dynamic describe);
    }

    [OrchardFeature("Orchard.Scripting.Rules")]
    public class ConditionActions : IActionProvider {
        private readonly IEnumerable<IScriptExpressionEvaluator> _evaluators;

        public ConditionActions(IEnumerable<IScriptExpressionEvaluator> evaluators) {
            _evaluators = evaluators;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic describe) {
            Func<dynamic, LocalizedString> display = context => new LocalizedString(context.Properties["description"]);

            describe.For("Condition", T("Conditions"), T("Conditions"))
                .Element("ScriptCondition", T("Script condition"), T("Evaluates a boolean using a scripting engine."), (Func<dynamic, bool>)Evaluate, display, "ScriptCondition");
        }

        private bool Evaluate(dynamic context) {
            var evaluator = _evaluators.FirstOrDefault();
            if (evaluator == null) {
                throw new OrchardException(T("There are currently no scripting engine enabled"));
            }

            var condition = context.Properties["condition"];

            // assume condition as True if empty
            if (!String.IsNullOrWhiteSpace(condition)) {
                var result = evaluator.Evaluate(condition, new List<IGlobalMethodProvider>());
                if (!(result is bool)) {
                    throw new OrchardException(T("Expression is not a boolean value"));
                }

                return (bool) result;
            }

            return true;
        }
    }
}