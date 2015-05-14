using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class QueryValidator : ElementValidator<Query> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        public QueryValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(Query element) {
            var settings = element.ValidationSettings;

            if (settings.Required == true)
                yield return _validationRuleFactory.Create<OptionRequired>(settings.CustomValidationMessage);
        }
    }
}