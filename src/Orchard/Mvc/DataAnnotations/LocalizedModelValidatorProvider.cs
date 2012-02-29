using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.Localization;

namespace Orchard.Mvc.DataAnnotations {
    public class LocalizedModelValidatorProvider : DataAnnotationsModelValidatorProvider {
        private static readonly Dictionary<Type, Func<ValidationAttribute, Localizer, ValidationAttribute>> _validationAttributes;

        static LocalizedModelValidatorProvider() {
            _validationAttributes = new Dictionary<Type, Func<ValidationAttribute, Localizer, ValidationAttribute>> {
                { typeof(RequiredAttribute),            (attribute, t) => new LocalizedRequiredAttribute((RequiredAttribute)attribute, t)},
                { typeof(RangeAttribute),               (attribute, t) => new LocalizedRangeAttribute((RangeAttribute)attribute, t)},
                { typeof(StringLengthAttribute),        (attribute, t) => new LocalizedStringLengthAttribute((StringLengthAttribute)attribute, t)},
                { typeof(RegularExpressionAttribute),   (attribute, t) => new LocalizedRegularExpressionAttribute((RegularExpressionAttribute)attribute, t)}
            };
        }

        protected override IEnumerable<ModelValidator> GetValidators(ModelMetadata metadata, ControllerContext context, IEnumerable<Attribute> attributes) {
            var localizedAttributes = new List<Attribute>();

            // overriden messages have their localization in the scope of the class they are applied to
            var tContainer = new Lazy<Localizer>(() => LocalizationUtilities.Resolve(context, (metadata.ContainerType ?? metadata.ModelType).FullName));

            foreach (var attribute in attributes) {
                Func<ValidationAttribute, Localizer, ValidationAttribute> localizedAttribute;

                // default translations use the attribute's scope, e.g., Orchard.Mvc.DataAnnotations.LocalizedRequiredAttribute
                var localAttribute = attribute;
                var tProvider = new Lazy<Localizer>(() => LocalizationUtilities.Resolve(context, localAttribute.GetType().FullName));

                var validationAttribute = attribute as ValidationAttribute;

                // substitute the attribute to its localized version if available
                if ( _validationAttributes.TryGetValue(attribute.GetType(), out localizedAttribute) ) {
                    localizedAttributes.Add(localizedAttribute((ValidationAttribute)attribute, tProvider.Value));
                }
                else {

                    // try to inject the localizer if it's an unkown validation attribute
                    if ( validationAttribute != null ) {
                        
                        var propertyInfo = validationAttribute.GetType().GetProperty("T", typeof(Localizer));
                        if ( propertyInfo != null ) {
                            propertyInfo.SetValue(attribute, tProvider.Value, null);
                        }
                    }
                    
                    if ( attribute is DisplayNameAttribute ) {
                        metadata.DisplayName = tContainer.Value(metadata.DisplayName).Text;
                    }

                    localizedAttributes.Add(attribute);
                }
            }

            var result = base.GetValidators(metadata, context, localizedAttributes);

            return result;
        }
    }
}