using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;

namespace Orchard.Core.Common.Models {
    public class IdentityPart : ContentPart<IdentityPartRecord> {
        public string Identifier {
            get { return this.As<InfosetPart>().Get<IdentityPart>("Identifier"); }
            set {
                this.As<InfosetPart>().Set<IdentityPart>("Identifier", value);
                Record.Identifier = value;
            }
        }
    }
}
