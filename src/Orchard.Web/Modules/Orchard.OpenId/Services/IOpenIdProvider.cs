namespace Orchard.OpenId.Services {
    public interface IOpenIdProvider : IDependency {
        string AuthenticationType { get; }
        string DisplayName { get; }
        bool IsValid { get; }
        string Name { get; }
    }
}
