namespace Orchard.Services {
    /// <summary>
    /// Provides access to the user host address.
    /// </summary>
    public interface IClientHostAddressAccessor : IDependency {
        string GetClientAddress();
    }
}