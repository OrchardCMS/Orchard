using Orchard.ContentManagement.Records;

namespace Orchard.Tests.Models.Records {
    public class DeltaRecord : ContentPartRecord {
        public virtual string Quux { get; set; }
    }
}