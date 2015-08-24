using System.ComponentModel;

namespace Orchard.Localization {
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute {
        public LocalizedDisplayNameAttribute() {}
        public LocalizedDisplayNameAttribute(string displayName) : base(displayName) {}
    }
}
