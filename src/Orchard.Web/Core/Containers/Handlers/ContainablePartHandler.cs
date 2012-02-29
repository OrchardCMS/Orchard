using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Core.Containers.Models;

namespace Orchard.Core.Containers.Handlers {
    public class ContainablePartHandler : ContentHandler {
        public ContainablePartHandler(IRepository<ContainablePartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}