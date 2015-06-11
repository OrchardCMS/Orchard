using Orchard.DynamicForms.Services.Models;
using Orchard.Events;

namespace Orchard.DynamicForms.Services {
    public interface IBindingProvider : IEventHandler {
        void Describe(BindingDescribeContext context);
    }
}