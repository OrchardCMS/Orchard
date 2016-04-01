using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Orchard.DynamicForms.Services.Models {
    public class FormValidatedEventContext : FormEventContext {
        public ModelStateDictionary ModelState { get; set; }

        public int ContentIdToEdit { get; set; }
    }
}