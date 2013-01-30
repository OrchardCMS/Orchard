using Orchard.ContentManagement.Records;

namespace Contrib.Taxonomies.Models {
    /// <summary>
    /// Contains the properties for the Taxonomy Menu Widget
    /// </summary>
    public class TaxonomyMenuPartRecord : ContentPartRecord {
        public virtual TaxonomyPartRecord TaxonomyPartRecord { get; set; }
        public virtual TermPartRecord TermPartRecord { get; set; }
        public virtual bool DisplayTopMenuItem { get; set; }
        public virtual int LevelsToDisplay { get; set; }
        public virtual bool DisplayContentCount { get; set; }
        public virtual bool HideEmptyTerms { get; set; }
    }
}
