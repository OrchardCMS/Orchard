using System.Collections.Generic;
using Orchard.DynamicForms.Elements;
using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.ValidationRules;
using Orchard.Layouts.Services;

namespace Orchard.DynamicForms.Validators {
    public class NumFieldValidator : ElementValidator<NumField> {
        private readonly IValidationRuleFactory _validationRuleFactory;
        private readonly ICultureAccessor _cultureAccessor;
        public NumFieldValidator(IValidationRuleFactory validationRuleFactory, ICultureAccessor cultureAccessor) {
            _validationRuleFactory = validationRuleFactory;
            _cultureAccessor = cultureAccessor;
        }

        protected override IEnumerable<IValidationRule> GetValidationRules(NumField element) {
            var settings = element.ValidationSettings;

            if (settings.IsRequired == true)
                yield return _validationRuleFactory.Create<Required>(settings.CustomValidationMessage);

            if (settings.Min != null || settings.Max != null) {
                yield return _validationRuleFactory.Create<Range>(r => {
                    r.Min = settings.Min;
                    r.Max = settings.Max;
                    r.Scale = settings.Scale;
                    r.Culture = _cultureAccessor.CurrentCulture;
                    r.ErrorMessage = settings.CustomValidationMessage;
                });
            }
        }
    }
}