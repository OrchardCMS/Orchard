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
        public string DisplayAlias {
            get { return Retrieve(x => x.DisplayAlias); }
            set { Store(x => x.DisplayAlias, value); }
        }

        public string Path {
            get { return DisplayAlias; }
        }
    }
}
