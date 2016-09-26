using Orchard.DynamicForms.Services.Models;
using Orchard.Localization;
using Orchard.Tokens;

namespace Orchard.DynamicForms.Services {
    public abstract class ValidationRule : Component, IValidationRule {
        public string ErrorMessage { get; set; }
        public abstract void Validate(ValidateInputContext context);
        public virtual void RegisterClientAttributes(RegisterClientValidationAttributesContext context) { }
        public ITokenizer Tokenizer { get; set; }

        protected string Tokenize(string errorMessage, ValidationContext context) {
            return Tokenizer.Replace(errorMessage, null);
        }
    }
}