using System.Collections.Generic;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.ViewModels {
    public class BlueprintsIndexViewModel {
        public IList<ElementEntry> Blueprints { get; set; }
        public AdminIndexOptions Options { get; set; }
    }

    public class ElementEntry {
        public ElementBlueprint Blueprint { get; set; }
        public bool IsChecked { get; set; }
    }

    public class AdminIndexOptions {
        public ElementsBulkAction BulkAction { get; set; }
    }

    public enum ElementsBulkAction {
        None,
        Delete
    }
}