using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;
using Orchard.Layouts.Services;

namespace Orchard.Layouts.Framework.Drivers {
    public interface IElementDriver : IDependency {
        int Priority { get; }
        EditorResult BuildEditor(ElementEditorContext context);
        EditorResult UpdateEditor(ElementEditorContext context);
        void Displaying(ElementDisplayContext context);
        void LayoutSaving(ElementSavingContext context);
        void Removing(ElementRemovingContext context);
        void Indexing(ElementIndexingContext context);
        void BuildDocument(BuildElementDocumentContext context);
        void Exporting(ExportElementContext context);
        void Importing(ImportElementContext context);
    }
}