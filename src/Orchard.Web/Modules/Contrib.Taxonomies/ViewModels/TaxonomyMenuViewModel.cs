using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Contrib.Taxonomies.ViewModels {
    public class TaxonomyMenuViewModel {
        public SelectList AvailableTaxonomies { get; set; }

        [Required, Range(0, int.MaxValue, ErrorMessage = "You must select a taxonomy")]
        public int SelectedTaxonomyId { get; set; }
        public int SelectedTermId { get; set; }

        public bool DisplayTopMenuItem { get; set; }
        [Required, Range(0, 99)]
        public int LevelsToDisplay { get; set; }
        public bool DisplayContentCount { get; set; }
        public bool HideEmptyTerms { get; set; }
    }
}
