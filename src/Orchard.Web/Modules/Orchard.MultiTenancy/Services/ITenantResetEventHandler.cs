using Orchard.Events;

namespace Orchard.MultiTenancy.Services {
    /// <summary>
    /// An event handler interface that allows implementers to execute code when a tenant is being reset.
    /// </summary>
    public interface ITenantResetEventHandler : IEventHandler {
        void Resetting();
    }
}