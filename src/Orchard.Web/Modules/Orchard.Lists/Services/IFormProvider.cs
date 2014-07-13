using Orchard.Events;

namespace Orchard.Lists.Services {
    public interface IFormProvider : IEventHandler {
        void Describe(dynamic context);
    }
}