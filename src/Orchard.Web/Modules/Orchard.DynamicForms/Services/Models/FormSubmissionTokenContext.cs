using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.DynamicForms.Elements;
using Orchard.ContentManagement;

namespace Orchard.DynamicForms.Services.Models {
    public class FormSubmissionTokenContext {
        public Form Form { get; set; }
        public ModelStateDictionary ModelState { get; set; }
        public NameValueCollection PostedValues { get; set; }
        public ContentItem CreatedContent { get; set; }
    }
}
