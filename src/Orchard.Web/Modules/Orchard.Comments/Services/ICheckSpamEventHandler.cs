using Orchard.Events;

namespace Orchard.Comments.Services {
    /// <summary>
    /// Stub interface for Orchard.AntiSpam.EventHandlers.ISpamEventHandler
    /// </summary>
    public interface ICheckSpamEventHandler : IEventHandler {
        void CheckSpam(dynamic context);
    }
}