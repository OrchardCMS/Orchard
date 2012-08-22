using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Localization;

namespace Orchard.Rules.ViewModels {
    public class EditRuleViewModel {
        public int Id { get; set; }
        [Required, StringLength(1024)]
        public string Name { get; set; }
        public bool Enabled { get; set; }

        public IEnumerable<EventEntry> Events { get; set; }
        public IEnumerable<ActionEntry> Actions { get; set; }
    }

    public class ActionEntry {
        public int ActionRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString DisplayText { get; set; }
    }

    public class EventEntry {
        public int EventRecordId { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public LocalizedString DisplayText { get; set; }
    }
}