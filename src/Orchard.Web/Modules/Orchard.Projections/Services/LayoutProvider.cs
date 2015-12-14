using Orchard.Events;
using Orchard.Projections.Descriptors.Layout;

namespace Orchard.Projections.Services {
    public interface ILayoutProvider : IEventHandler {
        void Describe(DescribeLayoutContext describe);
    }
}