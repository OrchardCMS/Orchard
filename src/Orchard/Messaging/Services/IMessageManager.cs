using System.Collections.Generic;
using Orchard.ContentManagement.Records;

namespace Orchard.Messaging.Services {
    public interface IMessageManager : IDependency {
        /// <summary>
        /// Sends a message to a channel using a user content item as the recipient
        /// </summary>
        void Send(ContentItemRecord recipient, string type, string service, Dictionary<string, string> properties = null);

        /// <summary>
        /// Sends a message to a channel using a comma-separated list of recipient addresses
        /// </summary>
        void Send(string recipientAddresses, string type, string service, Dictionary<string, string> properties = null);

        /// <summary>
        /// Whether at least one channel is active on the current site
        /// </summary>
        bool HasChannels();

        /// <summary>
        /// Provides a list of all the current available channel services
        /// </summary>
        IEnumerable<string> GetAvailableChannelServices();
    }
}
