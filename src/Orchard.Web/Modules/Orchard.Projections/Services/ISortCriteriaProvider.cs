using Orchard.Events;
using Orchard.Projections.Descriptors.SortCriterion;

namespace Orchard.Projections.Services {
    public interface ISortCriterionProvider : IEventHandler {
        void Describe(DescribeSortCriterionContext describe);
    }
}