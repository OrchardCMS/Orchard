using System.Web.Mvc;

namespace Orchard.DynamicForms.Services.Models {
    public class FormValidatingEventContext : FormEventContext {
        public ModelStateDictionary ModelState { get; set; }
    }
}