using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Models {
    public class EpsilonRecord : ContentPartVersionRecord {
        public virtual string Quad { get; set; }
    }
}