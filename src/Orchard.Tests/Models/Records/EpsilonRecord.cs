using Orchard.ContentManagement.Records;

namespace Orchard.Tests.Models.Stubs {
    public class EpsilonRecord : ContentPartVersionRecord {
        public virtual string Quad { get; set; }
    }
}