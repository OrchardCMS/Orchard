using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.ContentPermissions.Models {
    public class ContentPermissionsPartRecord : ContentPartRecord {

        /// <summary>
        /// Whether the access control should be applied for the content item
        /// </summary>
        public virtual bool Enabled { get; set; }

        [StringLengthMax]
        public virtual string ViewContent { get; set; }
        [StringLengthMax]
        public virtual string ViewOwnContent { get; set; }
        [StringLengthMax]
        public virtual string PublishContent { get; set; }
        [StringLengthMax]
        public virtual string PublishOwnContent { get; set; }
        [StringLengthMax]
        public virtual string EditContent { get; set; }
        [StringLengthMax]
        public virtual string EditOwnContent { get; set; }
        [StringLengthMax]
        public virtual string DeleteContent { get; set; }
        [StringLengthMax]
        public virtual string DeleteOwnContent { get; set; }
    
    }
}