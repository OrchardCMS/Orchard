using System.Collections.Generic;

namespace Orchard.DynamicForms.Services.Models {
    public class RegisterClientValidationAttributesContext : ValidationContext {
        public RegisterClientValidationAttributesContext() {
            ClientAttributes = new Dictionary<string, string>();
        }
        public IDictionary<string, string> ClientAttributes { get; set; }
    }
}