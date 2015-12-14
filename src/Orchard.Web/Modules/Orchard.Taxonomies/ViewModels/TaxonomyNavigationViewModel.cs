using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Orchard.Taxonomies.ViewModels {
    public class TaxonomyNavigationViewModel {
        public SelectList AvailableTaxonomies { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "You must select a taxonomy")]
        public int SelectedTaxonomyId { get; set; }
        public int SelectedTermId { get; set; }

        public bool DisplayTopMenuItem { get; set; }
        public bool DisplayContentCount { get; set; }
        public bool HideEmptyTerms { get; set; }
        public int LevelsToDisplay { get; set; }
    }
}
