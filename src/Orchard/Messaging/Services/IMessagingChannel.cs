using System.Collections.Generic;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Services {
    public interface IMessagingChannel : IDependency {
        /// <summary>
        /// Actually sends the message though this channel
        /// </summary>
        void SendMessage(MessageContext message);

        /// <summary>
        /// Provides all the handled services, the user can choose from when receving messages
        /// </summary>
        IEnumerable<string> GetAvailableServices();
    }
}
