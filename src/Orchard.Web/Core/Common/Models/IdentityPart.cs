using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class IdentityPart : ContentPart<IdentityPartRecord> {
        public string Identifier {
            get { return Record.Identifier; }
            set { Record.Identifier = value; }
        }
    }
}
