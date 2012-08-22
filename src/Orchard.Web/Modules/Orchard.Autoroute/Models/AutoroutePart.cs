using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Autoroute.Models {
    public class AutoroutePart : ContentPart<AutoroutePartRecord>, IAliasAspect {

        public string CustomPattern {
            get { return Record.CustomPattern; }
            set { Record.CustomPattern = value; }
        }

        public bool UseCustomPattern {
            get { return Record.UseCustomPattern; }
            set { Record.UseCustomPattern = value; }
        }

        public string DisplayAlias {
            get { return Record.DisplayAlias; }
            set { Record.DisplayAlias = value; }
        }

        public string Path {
            get { return DisplayAlias; }
        }
    }
}
