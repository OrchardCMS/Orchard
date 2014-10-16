using System;

namespace Orchard.DynamicForms.Services {
    public class ValidationRuleFactory : Component, IValidationRuleFactory {
        public TRule Create<TRule>(Action<TRule> setup = null) where TRule : ValidationRule, new() {
            return Create(errorMessage: null, setup: setup);
        }

        public TRule Create<TRule>(string errorMessage = null, Action<TRule> setup = null) where TRule : ValidationRule, new() {
            var rule = new TRule {
                T = T,
                Logger = Logger,
                ErrorMessage = errorMessage
            };

            if (setup != null)
                setup(rule);

            return rule;
        }
    }
}