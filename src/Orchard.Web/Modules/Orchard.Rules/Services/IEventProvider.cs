using Orchard.Events;
using Orchard.Rules.Models;

namespace Orchard.Rules.Services {
    public interface IEventProvider : IEventHandler {
        void Describe(DescribeEventContext describe);
    }
}