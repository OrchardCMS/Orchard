using Orchard.Models.Records;

namespace Orchard.Tests.Models.Records {
    public class DeltaRecord : ContentPartRecord {
        public virtual string Quux { get; set; }
    }
}