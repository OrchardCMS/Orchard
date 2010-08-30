using System.Collections.Generic;
using Orchard.Messaging.Models;

namespace Orchard.Messaging.Services {
    public interface IMessageManager : IDependency {
        /// <summary>
        /// Sends a message without using the queue
        /// </summary>
        void Send(Message message);

        /// <summary>
        /// Wether at least one channel is active on the current site
        /// </summary>
        bool HasChannels();

        /// <summary>
        /// Provides a list of all the current available channel services
        /// </summary>
        IEnumerable<string> GetAvailableChannelServices();
    }
}
