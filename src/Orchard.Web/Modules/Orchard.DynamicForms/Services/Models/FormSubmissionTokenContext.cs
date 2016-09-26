using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;

namespace Orchard.DynamicForms.Services.Models {
    public class FormSubmissionTokenContext {
        public Form Form { get; set; }
        public ModelStateDictionary ModelState { get; set; }
        public NameValueCollection PostedValues { get; set; }
    }
}