using Orchard.ContentManagement.Records;

namespace Orchard.Core.Common.Records {
    public class BodyRecord : ContentPartRecord {
        public virtual string Text { get; set; }
        public virtual string Format { get; set; }
    }
}