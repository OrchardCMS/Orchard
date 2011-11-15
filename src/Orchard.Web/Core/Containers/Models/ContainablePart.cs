using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Orchard.Core.Containers.Models {
    public class ContainablePart : ContentPart<ContainablePartRecord> {
        public int Weight {
            get { return Record.Weight; }
            set { Record.Weight = value; }
        }
    }

    public class ContainablePartRecord : ContentPartRecord {
        public virtual int Weight { get; set; }
    }
}