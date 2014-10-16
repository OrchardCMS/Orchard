using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class CheckBoxValidator : ElementValidator<CheckBox> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        public CheckBoxValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(CheckBox element) {
            var settings = element.ValidationSettings;

            if (settings.IsMandatory == true)
                yield return _validationRuleFactory.Create<Mandatory>(settings.CustomValidationMessage);
        }
    }
}