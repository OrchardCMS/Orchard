using System;
using Glimpse.Core.Message;
using Orchard.ContentManagement;
using Orchard.Glimpse.Models;
using Orchard.Security.Permissions;

namespace Orchard.Glimpse.Tabs.Authorizer {
    public class AuthorizerMessage : MessageBase, IDurationMessage {
        public string Message { get; set; }
        public IContent Content { get; set; }
        public Permission Permission { get; set; }
        public bool Result { get; set; }
        public TimeSpan Duration { get; set; }
    }
}