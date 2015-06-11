using System.Collections.Generic;

namespace Orchard.Messaging.Services {
    public interface IMessageChannel : IDependency {
        void Process(IDictionary<string, object> parameters);
    }
}