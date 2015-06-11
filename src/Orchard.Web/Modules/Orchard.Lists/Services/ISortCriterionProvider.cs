using Orchard.Events;

namespace Orchard.Lists.Services {
    public interface ISortCriterionProvider : IEventHandler {
        void Describe(dynamic describe);
    }
}