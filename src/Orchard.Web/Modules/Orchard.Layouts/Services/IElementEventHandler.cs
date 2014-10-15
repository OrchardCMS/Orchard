using Orchard.Events;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Drivers;

namespace Orchard.Layouts.Services {
    public interface IElementEventHandler : IEventHandler {
        void Creating(ElementCreatingContext context);
        void Created(ElementCreatedContext context);
        void Displaying(ElementDisplayContext context);
        void BuildEditor(ElementEditorContext context);
        void UpdateEditor(ElementEditorContext context);
    }
}