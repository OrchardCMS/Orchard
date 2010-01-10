using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Records;
using Orchard.Pages.Controllers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Pages.Models {
    [UsedImplicitly]
    public class PageHandler : ContentHandler {
        public PageHandler(IRepository<PageRecord> repository) {
            Filters.Add(new ActivatingFilter<Page>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(PageDriver.ContentType.Name));
            Filters.Add(new StorageFilter<PageRecord>(repository));
        }

    }
}