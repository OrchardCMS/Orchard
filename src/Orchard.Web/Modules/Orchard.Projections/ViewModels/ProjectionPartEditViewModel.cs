using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Projections.ViewModels {
    public class ProjectionPartEditViewModel {

        [Required, Range(0, int.MaxValue)]
        public int Items { get; set; }
        
        [Required, Range(0, int.MaxValue)]
        public int ItemsPerPage { get; set; }
        
        [Required, Range(0, int.MaxValue),DisplayName("Offset")]
        public int Skip { get; set; }

        public string PagerSuffix { get; set; }
        
        public bool DisplayPager { get; set; }
        
        [Required, Range(0, int.MaxValue)]
        public int MaxItems { get; set; }
        
        [Required(ErrorMessage = "You must select a Query and a Layout")]
        public string QueryLayoutRecordId { get; set; }

        public IEnumerable<QueryRecordEntry> QueryRecordEntries { get; set; }
        public IEnumerable<QueryRecordFilterEntry> QueryRecordIdFilterEntries { get; set; }

        public int PartId { get; set; }

        // manage Lock items
        public bool LockEditingItems { get; set; }
        public bool LockEditingSkip { get; set; }
        public bool LockEditingMaxItems { get; set; }
        public bool LockEditingPagerSuffix { get; set; }
        public bool LockEditingDisplayPager { get; set; }

    }

    public class QueryRecordEntry {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<LayoutRecordEntry> LayoutRecordEntries { get; set; }
    }

    public class LayoutRecordEntry {
        public int Id { get; set; }
        public string Description { get; set; }
    }
    public class QueryRecordFilterEntry {
        public string Id { get; set; }
        public string LayoutId { get; set; }
    }
}