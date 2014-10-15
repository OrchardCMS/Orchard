using System.Collections.Generic;
using Orchard.DynamicForms.Elements;

namespace Orchard.DynamicForms.Services.Models {
    public class RegisterClientValidationAttributesEventContext {
        public RegisterClientValidationAttributesEventContext() {
            ClientAttributes = new Dictionary<string, string>();
        }
        public IDictionary<string, string> ClientAttributes { get; set; }
        public FormElement Element { get; set; }

    }
}