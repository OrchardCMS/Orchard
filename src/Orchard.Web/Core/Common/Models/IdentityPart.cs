using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class IdentityPart : ContentPart<IdentityPartRecord> {
        public string Identifier {
            get { return Get("Identifier"); }
            set {
                Set("Identifier", value);
                Record.Identifier = value;
            }
        }
    }
}
