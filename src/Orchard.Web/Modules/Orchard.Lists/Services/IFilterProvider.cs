using Orchard.Events;

namespace Orchard.Lists.Services {
    public interface IFilterProvider : IEventHandler {
        void Describe(dynamic describe);
    }
}