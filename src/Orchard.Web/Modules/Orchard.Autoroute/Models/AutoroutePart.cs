using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.Autoroute.Models {
    public class AutoroutePart : ContentPart<AutoroutePartRecord>, IAliasAspect {

        public string CustomPattern {
            get { return this.As<InfosetPart>().Get<AutoroutePart>("CustomPattern"); }
            set {
                this.As<InfosetPart>().Set<AutoroutePart>("CustomPattern", value);
                Record.CustomPattern = value;
            }
        }

        public bool UseCustomPattern {
            get { return bool.Parse(this.As<InfosetPart>().Get<AutoroutePart>("UseCustomPattern")); }
            set {
                this.As<InfosetPart>().Set<AutoroutePart>("UseCustomPattern", value.ToString());
                Record.UseCustomPattern = value;
            }
        }

        public string DisplayAlias {
            get { return this.As<InfosetPart>().Get<AutoroutePart>("DisplayAlias"); }
            set {
                this.As<InfosetPart>().Set<AutoroutePart>("DisplayAlias", value);
                Record.DisplayAlias = value;
            }
        }

        public string Path {
            get { return DisplayAlias; }
        }
    }
}
