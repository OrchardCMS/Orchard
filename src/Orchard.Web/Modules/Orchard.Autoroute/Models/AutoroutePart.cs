using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Autoroute.Models {
    public class AutoroutePart : ContentPart<AutoroutePartRecord>, IAliasAspect {

        public string CustomPattern {
            get { return Retrieve(x => x.CustomPattern); }
            set { Store(x => x.CustomPattern, value); }
        }
        public bool UseCustomPattern {
            get { return Retrieve(x => x.UseCustomPattern); }
            set { Store(x => x.UseCustomPattern, value); }
        }
        public bool UseCulturePattern {
            get { return Retrieve(x => x.UseCulturePattern); }
            set { Store(x => x.UseCulturePattern, value); }
        }
        public string DisplayAlias {
            get { return Retrieve(x => x.DisplayAlias); }
            set { Store(x => x.DisplayAlias, value); }
        }

        public bool PromoteToHomePage {
            get { return this.Retrieve(x => x.PromoteToHomePage); }
            set { this.Store(x => x.PromoteToHomePage, value); }
        }

        public string Path {
            get { return DisplayAlias; }
        }
    }
}
