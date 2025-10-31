namespace Orchard.OpenId.Services
{
    public interface IOpenIdAuthenticationService : IDependency
    {
        bool IsLocalUser();
    }
}
