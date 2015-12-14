using Orchard.ContentManagement;
using Orchard.Events;

namespace Orchard.AntiSpam.Services {
    public interface ISpamEventHandler : IEventHandler {
        void SpamReported(IContent content);
        void HamReported(IContent content);
    }
}
