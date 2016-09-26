using System;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Services {
    public class ValidationRuleFactory : Component, IValidationRuleFactory {
        private readonly ITokenizer _tokenizer;
        public ValidationRuleFactory(ITokenizer tokenizer) {
            _tokenizer = tokenizer;
        }

        public TRule Create<TRule>(Action<TRule> setup = null) where TRule : ValidationRule, new() {
            return Create(errorMessage: null, setup: setup);
        }

        public TRule Create<TRule>(string errorMessage = null, Action<TRule> setup = null) where TRule : ValidationRule, new() {
            var rule = new TRule {
                T = T,
                Logger = Logger,
                ErrorMessage = errorMessage,
                Tokenizer = _tokenizer
            };

            if (setup != null)
                setup(rule);

            return rule;
        }
    }
}