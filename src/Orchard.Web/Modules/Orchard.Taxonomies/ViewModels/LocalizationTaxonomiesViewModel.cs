using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Orchard.Taxonomies.ViewModels {
    public class LocalizationTaxonomiesViewModel {
        public IEnumerable<string> Cultures { get;set;}
        public string SelectedCulture { get; set; }
        public string ContentType { get; set; }
        public string ContentPart { get; set; }
        public string FieldName { get; set; }
        public LocalizationTaxonomyFieldSettings Settings { get; set; }
        public string Prefix { get; set; }
    }
}