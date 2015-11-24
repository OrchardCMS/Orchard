using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class UrlFieldValidator : ElementValidator<UrlField> {
        private readonly IValidationRuleFactory _validationRuleFactory;

        public UrlFieldValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(UrlField element) {
            var settings = element.ValidationSettings;

            if (settings.IsRequired == true)
                yield return _validationRuleFactory.Create<Required>(settings.CustomValidationMessage);

            if (settings.MaximumLength != null) {
                yield return _validationRuleFactory.Create<StringLength>(r => {
                    r.Maximum = settings.MaximumLength;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
        }
    }
}