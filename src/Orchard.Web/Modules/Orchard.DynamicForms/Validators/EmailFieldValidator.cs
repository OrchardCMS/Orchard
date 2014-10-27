using System;
using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;

namespace Orchard.DynamicForms.Validators {
    public class EmailFieldValidator : ElementValidator<EmailField> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        public EmailFieldValidator(IValidationRuleFactory validationRuleFactory) {
            _validationRuleFactory = validationRuleFactory;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(EmailField element) {
            var settings = element.ValidationSettings;

            if (settings.IsRequired == true)
                yield return _validationRuleFactory.Create<Required>(settings.CustomValidationMessage);

            if (settings.MaximumLength != null) {
                yield return _validationRuleFactory.Create<StringLength>(r => {
                    r.Maximum = settings.MaximumLength;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }

            yield return _validationRuleFactory.Create<EmailAddress>(settings.CustomValidationMessage);

            if (!String.IsNullOrWhiteSpace(settings.CompareWith)) {
                yield return _validationRuleFactory.Create<Compare>(r => {
                    r.TargetName = settings.CompareWith;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
        }
    }
}