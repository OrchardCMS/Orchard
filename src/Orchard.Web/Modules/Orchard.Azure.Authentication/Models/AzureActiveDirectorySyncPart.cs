using Orchard.ContentManagement;
using System;

namespace Orchard.Azure.Authentication.Models {
    public class AzureActiveDirectorySyncPart : ContentPart<AzureActiveDirectorySyncPartRecord> {
        public int UserId {
            get { return this.Retrieve(x => x.UserId); }
            set { this.Store(x => x.UserId, value); }
        }

        public DateTime LastSyncedUtc {
            get { return this.Retrieve(x => x.LastSyncedUtc); }
            set { this.Store(x => x.LastSyncedUtc, value); }
        }
    }
}