namespace Orchard.MessageBus.Services
{
    public interface IHostNameProvider : IDependency
    {
        string GetHostName();
    }
}
