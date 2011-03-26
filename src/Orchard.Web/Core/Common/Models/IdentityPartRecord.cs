using Orchard.ContentManagement.Records;

namespace Orchard.Core.Common.Models {
    public class IdentityPartRecord : ContentPartRecord {
        public virtual string Identifier { get; set; }
    }
}