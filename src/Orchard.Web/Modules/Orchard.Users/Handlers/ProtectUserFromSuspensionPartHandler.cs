using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Users.Models;

namespace Orchard.Users.Handlers {
    // [OrchardFeature("AutomatedUserModeration")]?
    public class ProtectUserFromSuspensionPartHandler : ContentHandler {
        public ProtectUserFromSuspensionPartHandler(
            IRepository<ProtectUserFromSuspensionPartRecord> repository) {

            Filters.Add(new ActivatingFilter<ProtectUserFromSuspensionPart>("User"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}