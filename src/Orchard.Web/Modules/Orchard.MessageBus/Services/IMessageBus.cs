using System;

namespace Orchard.MessageBus.Services
{
    public interface IMessageBus : ISingletonDependency
    {
        void Subscribe(string channel, Action<string, string> handler);
        void Publish(string channel, string message);
    }
}
