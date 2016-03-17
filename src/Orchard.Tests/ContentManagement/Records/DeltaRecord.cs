using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Records {
    public class DeltaRecord : ContentPartRecord {
        public virtual string Quux { get; set; }
    }
}