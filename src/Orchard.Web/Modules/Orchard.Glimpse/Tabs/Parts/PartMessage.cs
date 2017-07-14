using System;
using Glimpse.Core.Message;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Glimpse.Models;

namespace Orchard.Glimpse.Tabs.Parts {
    public class PartMessage : MessageBase, IDurationMessage {
        public int ContentId { get; set; }
        public ContentPartDefinition PartDefinition { get; set; }
        public string DisplayType { get; set; }
        public string ContentName { get; set; }
        public string ContentType { get; set; }
        public TimeSpan Duration { get; set; }
    }
}