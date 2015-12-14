using Orchard.Events;
using Orchard.Projections.Descriptors.Property;

namespace Orchard.Projections.Services {
    public interface IPropertyProvider : IEventHandler {
        void Describe(DescribePropertyContext describe);
    }
}