using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.Layouts.Services {
    public abstract class ElementEventHandlerBase : Component, IElementEventHandler {
        public virtual void Creating(ElementCreatingContext context) { }
        public virtual void Created(ElementCreatedContext context) { }
        public virtual void CreatingDisplay(ElementCreatingDisplayShapeContext context) {}
        public virtual void Displaying(ElementDisplayingContext context) { }
        public virtual void Displayed(ElementDisplayedContext context) { }
        public virtual void BuildEditor(ElementEditorContext context) { }
        public virtual void UpdateEditor(ElementEditorContext context) { }
    }
}