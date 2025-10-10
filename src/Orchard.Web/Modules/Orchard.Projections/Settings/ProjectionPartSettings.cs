using System.Collections.Generic;
using System.ComponentModel;
using Orchard.Projections.ViewModels;

namespace Orchard.Projections.Settings {
    public class ProjectionPartSettings {
        public ProjectionPartSettings() {
            FilterQueryRecordsId = new List<string>();
        }

        public string QueryLayoutRecordId { get; set; }
        // saved identity part for import
        public string IdentityQueryLayoutRecord { get; set; }
        public IEnumerable<QueryRecordEntry> QueryRecordEntries { get; set; }
        public IEnumerable<string> FilterQueryRecordsId { get; set; }
        public string FilterQueryRecordId { get; set; }
        // saved identity part for import
        public string IdentityFilterQueryRecord { get; set; }
        public int Items { get; set; }
        public bool LockEditingItems { get; set; }
        [DisplayName("Offset")]
        public int Skip { get; set; }
        public bool LockEditingSkip { get; set; }
        public int MaxItems { get; set; }
        public bool LockEditingMaxItems { get; set; }
        public string PagerSuffix { get; set; }
        public bool LockEditingPagerSuffix { get; set; }
        public bool DisplayPager { get; set; }
        public bool LockEditingDisplayPager { get; set; }
    }
}