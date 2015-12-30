using Orchard.Events;
using Orchard.Rules.Models;

namespace Orchard.Rules.Services {
    public interface IActionProvider : IEventHandler {
        void Describe(DescribeActionContext describe);
    }
}