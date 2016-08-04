using Orchard.Events;
using Orchard.Projections.Descriptors.Filter;

namespace Orchard.Projections.Services {
    public interface IFilterProvider : IEventHandler {
        void Describe(DescribeFilterContext describe);
    }
}