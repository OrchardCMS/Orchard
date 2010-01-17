using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.Services;
using Orchard.Pages.Controllers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Pages.Services;

namespace Orchard.Pages.Models {
    [UsedImplicitly]
    public class PageHandler : ContentHandler {
        public PageHandler(IRepository<CommonVersionRecord> commonRepository, IPageService pageService, IRoutableService routableService) {
            Filters.Add(new ActivatingFilter<Page>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(PageDriver.ContentType.Name));
            Filters.Add(new StorageFilter<CommonVersionRecord>(commonRepository));

            OnCreating<Page>((context, page) =>
            {
                string slug = !string.IsNullOrEmpty(page.Slug)
                                  ? page.Slug
                                  : routableService.Slugify(page.Title);

                page.Slug = routableService.GenerateUniqueSlug(slug,
                                                       pageService.Get(PageStatus.Published).Where(
                                                           p => p.Slug.StartsWith(slug) && p.Id != page.Id).Select(
                                                           p => p.Slug));
            });
        }
    }
}