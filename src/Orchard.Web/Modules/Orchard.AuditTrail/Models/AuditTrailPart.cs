using Orchard.ContentManagement;

namespace Orchard.AuditTrail.Models {
    public class AuditTrailPart : ContentPart {
        public string Comment {
            get { return RetrieveVersioned<string>("Comment"); }
            set { StoreVersioned("Comment", value); }
        }

        public bool ShowComment { get; set; }
    }
}