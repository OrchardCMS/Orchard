using Orchard.ContentManagement.Records;

namespace Orchard.Core.Messaging.Models {
    public class MessageSettingsPartRecord : ContentPartRecord {
        /// <summary>
        /// Default service used for messages
        /// </summary>
        public virtual string DefaultChannelService { get; set; }
    
    }
}