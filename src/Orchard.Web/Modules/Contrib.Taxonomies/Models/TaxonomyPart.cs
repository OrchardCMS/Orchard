using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace Contrib.Taxonomies.Models {
    public class TaxonomyPart : ContentPart<TaxonomyPartRecord> {
        public string Name {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public string Slug {
            get { return this.As<AutoroutePart>().DisplayAlias; }
            set { this.As<AutoroutePart>().DisplayAlias = value; }
        }

        public string TermTypeName {
            get { return Record.TermTypeName; }
            set { Record.TermTypeName = value; }
        }
    }
}
