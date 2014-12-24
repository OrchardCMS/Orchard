using System.Collections.Specialized;
using System.Web.Mvc;
using Orchard.ContentManagement;

namespace Orchard.DynamicForms.Services.Models {
    public class ValidateInputContext : ValidationContext {
        public ModelStateDictionary ModelState { get; set; }
        public string AttemptedValue { get; set; }
        public NameValueCollection Values { get; set; }
        public IUpdateModel Updater { get; set; }
    }
}