using System;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Services {
    public interface IMessageChannel : IDependency, IDisposable {
        string Name { get; }
        void Send(QueuedMessage message);
    }
}