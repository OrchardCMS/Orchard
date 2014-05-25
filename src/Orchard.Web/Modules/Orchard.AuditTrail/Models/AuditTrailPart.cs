using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailPart : ContentPart {
        public string Comment {
            get { return this.Retrieve(x => x.Comment); }
            set { this.Store(x => x.Comment, value); }
        }
    }
}