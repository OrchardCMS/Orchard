using Orchard.Events;

namespace Orchard.AntiSpam.EventHandlers {
    public interface ICheckSpamEventHandler : IEventHandler {
        void CheckSpam(dynamic context);
    }
}