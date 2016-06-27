using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Localization;

namespace Orchard.Layouts.Framework.Elements {
    public class ElementDescriptor {
        public ElementDescriptor(Type elementType, string typeName, LocalizedString displayText, LocalizedString description, string category) {
            ElementType = elementType;
            TypeName = typeName;
            DisplayText = displayText;
            Description = description;
            Category = category;
            GetDrivers = Enumerable.Empty<IElementDriver>;
            CreatingDisplay = context => { };
            Displaying = context => { };
            Editor = context => { };
            UpdateEditor = context => { };
            LayoutSaving = context => { };
            Removing = context => { };
            Exporting = context => { };
            Exported = context => { };
            Importing = context => { };
            Imported = context => { };
            ImportCompleted = context => { };
            StateBag = new Dictionary<string, object>();
        }

        public LocalizedString DisplayText { get; set; }
        public LocalizedString Description { get; set; }
        public string ToolboxIcon { get; set; }
        public string Category { get; set; }
        public Type ElementType { get; set; }
        public string TypeName { get; set; }
        public Func<IEnumerable<IElementDriver>> GetDrivers { get; set; }
        public Action<ElementCreatingDisplayShapeContext> CreatingDisplay { get; set; }
        public Action<ElementDisplayingContext> Displaying { get; set; }
        public Action<ElementDisplayedContext> Displayed { get; set; }
        public Action<ElementEditorContext> Editor { get; set; }
        public Action<ElementEditorContext> UpdateEditor { get; set; }
        public Action<ElementSavingContext> LayoutSaving { get; set; }
        public Action<ElementRemovingContext> Removing { get; set; }
        public Action<ExportElementContext> Exporting { get; set; }
        public Action<ExportElementContext> Exported { get; set; }
        public Action<ImportElementContext> Importing { get; set; }
        public Action<ImportElementContext> Imported { get; set; }
        public Action<ImportElementContext> ImportCompleted { get; set; }
        public bool IsSystemElement { get; set; }
        public bool EnableEditorDialog { get; set; }
        public IDictionary<string, object> StateBag { get; set; }
    }
}