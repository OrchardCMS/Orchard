using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Records;

namespace Orchard.Messaging.Models {
    public class MessageSettingsPartRecord : ContentPartRecord {
        public const ushort DefaultChannelServiceLength = 64;

        /// <summary>
        /// Default service used for messages
        /// </summary>
        [StringLength(DefaultChannelServiceLength)]
        public virtual string DefaultChannelService { get; set; }
    
    }
}