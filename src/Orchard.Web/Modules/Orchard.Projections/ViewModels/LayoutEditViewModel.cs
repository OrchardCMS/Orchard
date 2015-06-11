using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Projections.Descriptors.Layout;
using Orchard.Projections.Models;

namespace Orchard.Projections.ViewModels {

    public class LayoutEditViewModel {
        public LayoutEditViewModel() {
            Properties = new List<PropertyEntry>();
            Display = (int)LayoutRecord.Displays.Content;
            DisplayType = "Summary";
        }

        public int Id { get; set; }
        public int QueryId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public LayoutDescriptor Layout { get; set; }
        public dynamic Form { get; set; }

        [Required]
        public int Display { get; set; }

        [Required, StringLength(64)]
        public string DisplayType { get; set; }

        public IEnumerable<PropertyEntry> Properties { get; set; }

        public int GroupPropertyId { get; set; }
    }
    
    public class PropertyEntry {
        public int PropertyRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string DisplayText { get; set; }
        public int Position { get; set; }
    }
}
