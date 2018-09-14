using System;
using Glimpse.Core.Message;
using Orchard.Glimpse.Models;
using Orchard.Widgets.Models;

namespace Orchard.Glimpse.Tabs.Widgets {
    public class WidgetMessage : MessageBase, IDurationMessage {
        public int ContentId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Zone { get; set; }
        public string Position { get; set; }
        public string TechnicalName { get; set; }
        public string EditUrl { get; set; }
        public LayerPart Layer { get; set; }
        public TimeSpan Duration { get; set; }
    }
}