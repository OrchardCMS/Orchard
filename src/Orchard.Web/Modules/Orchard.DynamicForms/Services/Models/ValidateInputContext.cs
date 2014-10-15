using System.Web.Mvc;
using Orchard.DynamicForms.Elements;

namespace Orchard.DynamicForms.Services.Models {
    public class ValidateInputContext {
        public FormElement Element { get; set; }
        public ModelStateDictionary ModelState { get; set; }
        public string FieldName { get; set; }
        public string AttemptedValue { get; set; }
    }
}