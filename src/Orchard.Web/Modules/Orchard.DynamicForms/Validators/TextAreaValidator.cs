using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class TextAreaValidator : ElementValidator<TextArea> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        public TextAreaValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(TextArea element) {
            var settings = element.ValidationSettings;

            if (settings.IsRequired == true)
                yield return _validationRuleFactory.Create<Required>(settings.CustomValidationMessage);

            if (settings.MinimumLength != null || settings.MaximumLength != null) {
                yield return _validationRuleFactory.Create<StringLength>(r => {
                    r.Minimum = settings.MinimumLength;
                    r.Maximum = settings.MaximumLength;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
        }
    }
}