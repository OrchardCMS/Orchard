using Orchard.ContentManagement;

namespace Orchard.Core.Common.Models {
    public class IdentityPart : ContentPart<IdentityPartRecord> {
        public string Identifier {
            get { return Retrieve(x => x.Identifier); }
            set { Store(x => x.Identifier, value); }
        }
    }
}
