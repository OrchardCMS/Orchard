using System.Web.Mvc;

namespace Orchard.DynamicForms.Services.Models {
    public class FormValidatedEventContext : FormEventContext {
        public ModelStateDictionary ModelState { get; set; }
    }
}