using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace Orchard.Autoroute.Models {
    public class AutoroutePart : ContentPart<AutoroutePartRecord>, IAliasAspect {

        public string CustomPattern {
            get { return Get("CustomPattern"); }
            set {
                Set("CustomPattern", value);
                Record.CustomPattern = value;
            }
        }

        public bool UseCustomPattern {
            get { return bool.Parse(Get("UseCustomPattern")); }
            set {
                Set("UseCustomPattern", value.ToString());
                Record.UseCustomPattern = value;
            }
        }

        public string DisplayAlias {
            get { return Get("DisplayAlias"); }
            set {
                Set("DisplayAlias", value);
                Record.DisplayAlias = value;
            }
        }

        public string Path {
            get { return DisplayAlias; }
        }
    }
}
