using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Projections.ViewModels {
    public class AdminEditViewModel {
        public int Id { get; set; }
        [Required, StringLength(1024)]
        public string Name { get; set; }

        public IEnumerable<FilterGroupEntry> FilterGroups { get; set; }
        public IEnumerable<SortCriterionEntry> SortCriteria { get; set; }
        public IEnumerable<LayoutEntry> Layouts { get; set; }
    }


    public class FilterGroupEntry {
        public int Id { get; set; }
        public IEnumerable<FilterEntry> Filters { get; set; }
    }

    public class FilterEntry {
        public int FilterRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
    }

    public class SortCriterionEntry {
        public int SortCriterionRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
    }

    public class LayoutEntry {
        public int LayoutRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
    }
}