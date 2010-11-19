using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace Orchard.Messaging.Services {
    public interface IMessageManager : IDependency {
        /// <summary>
        /// Sends a message to a channel
        /// </summary>
        void Send(ContentItemRecord recipient, string type, string service, Dictionary<string, string> properties = null);

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
