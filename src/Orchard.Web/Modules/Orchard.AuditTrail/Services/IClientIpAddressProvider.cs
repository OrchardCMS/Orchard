namespace Orchard.AuditTrail.Services {
    public interface IClientIpAddressProvider : IDependency {
        string GetClientIpAddress();
    }
}