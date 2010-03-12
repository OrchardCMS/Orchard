using System;

namespace Orchard.Themes {
    public class ThemedAttribute : Attribute {
        public ThemedAttribute() {
            Enabled = true;
        }
        public ThemedAttribute(bool enabled) {
            Enabled = enabled;
        }

        public bool Enabled { get; set; }
    }
}
