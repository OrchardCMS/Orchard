using System;

namespace Orchard.Widgets.Services {
    [Obsolete("Use Orchard.Conditions.Services.ConditionEvaluationContext instead.")]
    public class RuleContext {
        public string FunctionName { get; set; }
        public object[] Arguments { get; set; }
        public object Result { get; set; }
    }
}