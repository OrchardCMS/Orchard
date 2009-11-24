using Orchard.Models.Records;

namespace Orchard.Core.Common.Records {
    public class BodyRecord : ContentPartRecord {
        public virtual string Body { get; set; }
        public virtual string Format { get; set; }
    }
}