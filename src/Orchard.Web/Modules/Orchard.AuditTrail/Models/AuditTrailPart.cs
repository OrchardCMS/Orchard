using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailPart : ContentPart {
        public string LastComment {
            get { return this.Retrieve(x => x.LastComment); }
            set { this.Store(x => x.LastComment, value); }
        }
    }
}