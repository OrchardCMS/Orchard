using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.MediaProcessing.ViewModels {
    public class AdminEditViewModel {
        public int Id { get; set; }

        [Required, StringLength(1024)]
        public string Name { get; set; }

        public IEnumerable<FilterEntry> Filters { get; set; }
    }

    public class FilterEntry {
        public int FilterRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
    }
}