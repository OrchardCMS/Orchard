using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Orchard.ImportExport.Models {
    [OrchardFeature("Orchard.Deployment")]
    public class DeployablePartRecord : ContentPartVersionRecord {
        public virtual DateTime? ImportedPublishedUtc { get; set; }
        public virtual DateTime? UnpublishedUtc { get; set; }
        public virtual bool Latest { get; set; }
    }

    public class DeployablePart : ContentPart<DeployablePartRecord> {
        public DateTime? ImportedPublishedUtc {
            get { return Record.ImportedPublishedUtc; }
            set { Record.ImportedPublishedUtc = value; }
        }

        public DateTime? UnpublishedUtc {
            get { return Record.UnpublishedUtc; }
            set { Record.UnpublishedUtc = value; }
        }

        public bool Latest {
            get { return Record.Latest; }
            set { Record.Latest = value; }
        }
    }
}