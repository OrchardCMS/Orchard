using System.ComponentModel.DataAnnotations;

namespace Orchard.Layouts.Settings {
    public class LayoutTypePartSettings {
        [DataType("Flavor")]
        public string Flavor { get; set; }

        public bool IsTemplate { get; set; }

        public string DefaultLayoutState { get; set; }
    }
}