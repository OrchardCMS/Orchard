using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Contrib.Taxonomies.Models {
    /// <summary>
    /// This Content Part is used to create a link to TermContentItem records, so
    /// that the Content Manager can query them. It will be attached dynamically whenever
    /// a TaxonomyField is found on a Content Type
    /// </summary>
    public class TermsPart : ContentPart<TermsPartRecord> {
        public IList<TermContentItem> Terms { get { return Record.Terms; } }
    }
}