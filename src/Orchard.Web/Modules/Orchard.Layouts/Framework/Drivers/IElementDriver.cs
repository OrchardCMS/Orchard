using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Drivers {
    public interface IElementDriver : IDependency {
        int Priority { get; }
        EditorResult BuildEditor(ElementEditorContext context);
        EditorResult UpdateEditor(ElementEditorContext context);
        void Displaying(ElementDisplayContext context);
        void LayoutSaving(ElementSavingContext context);
        void ElementRemoving(ElementRemovingContext context);
    }
}