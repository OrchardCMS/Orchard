using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Drivers {
    public interface IElementDriver : IDependency {
        int Priority { get; }
        EditorResult BuildEditor(ElementEditorContext context);
        EditorResult UpdateEditor(ElementEditorContext context);
        void CreatingDisplay(ElementCreatingDisplayShapeContext context);
        void Displaying(ElementDisplayingContext context);
        void Displayed(ElementDisplayedContext context);
        void LayoutSaving(ElementSavingContext context);
        void Removing(ElementRemovingContext context);
        void Exporting(ExportElementContext context);
        void Exported(ExportElementContext context);
        void Importing(ImportElementContext context);
        void Imported(ImportElementContext context);
        void ImportCompleted(ImportElementContext context);
    }
}