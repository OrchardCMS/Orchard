using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Projections.Models {
    public class FieldIndexPartRecord : ContentPartRecord {
        public FieldIndexPartRecord() {
            StringFieldIndexRecords = new List<StringFieldIndexRecord>();
            IntegerFieldIndexRecords = new List<IntegerFieldIndexRecord>();
            DoubleFieldIndexRecords = new List<DoubleFieldIndexRecord>();
            DecimalFieldIndexRecords = new List<DecimalFieldIndexRecord>();
        }

        [CascadeAllDeleteOrphan]
        public virtual IList<StringFieldIndexRecord> StringFieldIndexRecords { get; set; }
        [CascadeAllDeleteOrphan]
        public virtual IList<IntegerFieldIndexRecord> IntegerFieldIndexRecords { get; set; }
        [CascadeAllDeleteOrphan]
        public virtual IList<DoubleFieldIndexRecord> DoubleFieldIndexRecords { get; set; }
        [CascadeAllDeleteOrphan]
        public virtual IList<DecimalFieldIndexRecord> DecimalFieldIndexRecords { get; set; }
    }
}