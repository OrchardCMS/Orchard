using System;
using Glimpse.Core.Message;
using Orchard.Glimpse.Models;

namespace Orchard.Glimpse.Tabs.Cache {
    public class CacheMessage : MessageBase, IDurationMessage {
        public string Action { get; set; }
        public string Key { get; set; }
        public object Value { get; set; }
        public string Result { get; set; }
        public TimeSpan? ValidFor { get; set; }
        public TimeSpan Duration { get; set; }
    }
}