using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Common.Records;
using Orchard.Core.Common.Services;
using Orchard.Localization;
using Orchard.Pages.Controllers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Pages.Services;
using Orchard.UI.Notify;

namespace Orchard.Pages.Models {
    [UsedImplicitly]
    public class PageHandler : ContentHandler {
        private readonly IPageService _pageService;
        private readonly IRoutableService _routableService;
        private readonly IOrchardServices _orchardServices;

        public PageHandler(IRepository<CommonVersionRecord> commonRepository, IPageService pageService, IRoutableService routableService, IOrchardServices orchardServices) {
            _pageService = pageService;
            _routableService = routableService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<Page>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<CommonAspect>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<ContentPart<CommonVersionRecord>>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<RoutableAspect>(PageDriver.ContentType.Name));
            Filters.Add(new ActivatingFilter<BodyAspect>(PageDriver.ContentType.Name));
            Filters.Add(new StorageFilter<CommonVersionRecord>(commonRepository));

            OnPublished<Page>((context, p) => ProcessSlug(p));
        }

        Localizer T { get; set; }

        private void ProcessSlug(Page page) {
            _routableService.FillSlug(page.As<RoutableAspect>());

            var slugsLikeThis = _pageService.Get(PageStatus.Published).Where(
                p => p.Slug.StartsWith(page.Slug, StringComparison.OrdinalIgnoreCase) &&
                p.Id != page.Id).Select(p => p.Slug);

            //todo: (heskew) need better messages
            if (slugsLikeThis.Count() > 0) {
                //todo: (heskew) need better messages
                var originalSlug = page.Slug;
                page.Slug = _routableService.GenerateUniqueSlug(page.Slug, slugsLikeThis);

                if (originalSlug != page.Slug)
                    _orchardServices.Notifier.Warning(T("A different page is already published with this same slug ({0}) so a unique slug ({1}) was generated for this page.", originalSlug, page.Slug));
            }
        }
    }
}