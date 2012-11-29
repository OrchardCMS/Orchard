using Orchard.Events;
using Orchard.Workflows.Models.Descriptors;

namespace Orchard.Workflows.Services {
    public interface IActivityProvider : IEventHandler {
        void Describe(DescribeActivityContext describe);
    }
}