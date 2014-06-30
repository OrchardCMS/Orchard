using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Orchard.ImportExport.Models {
    [OrchardFeature("Orchard.Deployment")]
    public class RecurringTaskPartRecord : ContentPartRecord {
        public virtual bool IsActive { get; set; }
        public virtual int RepeatFrequencyInMinutes { get; set; }
    }

    [OrchardFeature("Orchard.Deployment")]
    public class RecurringTaskPart : ContentPart<RecurringTaskPartRecord> {
        public bool IsActive {
            get { return Record.IsActive; }
            set { Record.IsActive = value; }
        }

        public int RepeatFrequencyInMinutes {
            get { return Record.RepeatFrequencyInMinutes; }
            set { Record.RepeatFrequencyInMinutes = value; }
        }
    }
}
