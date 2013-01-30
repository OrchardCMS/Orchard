using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using Contrib.Taxonomies.Models;

namespace Contrib.Taxonomies.ViewModels {
    public class TermWidgetViewModel {
        public SelectList AvailableTaxonomies { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "You must select a taxonomy")]
        public int SelectedTaxonomyId { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "You must select a term")]
        public int SelectedTermId { get; set; }

        public bool Ascending { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int Count { get; set; }
        public string FieldName { get; set; }
        public string OrderBy { get; set; }

        public TermWidgetPart Part { get; set; }
        public IEnumerable<string> ContentTypeNames { get; set; }
    }
}
