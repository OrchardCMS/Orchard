using System;
using Orchard.Layouts.Framework.Display;
using Orchard.Localization;

namespace Orchard.Layouts.Framework.Elements {
    public class ElementDescriptor {
        public ElementDescriptor(Type elementType, string typeName, LocalizedString displayText, string category) {
            ElementType = elementType;
            TypeName = typeName;
            DisplayText = displayText;
            Category = category;
            CreatingDisplay = context => {};
            Displaying = context => {};
        }

        public LocalizedString DisplayText { get; set; }
        public string Category { get; set; }
        public Type ElementType { get; set; }
        public string TypeName { get; set; }
        public Action<ElementCreatingDisplayShapeContext> CreatingDisplay { get; set; }
        public Action<ElementDisplayContext> Displaying { get; set; }
        public bool IsSystemElement { get; set; }
        public bool EnableEditorDialog { get; set; }
    }
}