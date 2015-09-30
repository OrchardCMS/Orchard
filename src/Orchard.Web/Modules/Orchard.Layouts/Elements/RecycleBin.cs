using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Elements {
    public class RecycleBin : Container {
        public override string Category { get { return null; } }
        public override bool IsSystemElement { get { return true; } }
    }
}