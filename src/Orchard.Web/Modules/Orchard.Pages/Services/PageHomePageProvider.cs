using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Pages.Routing;
using Orchard.Pages.ViewModels;
using Orchard.Services;

namespace Orchard.Pages.Services {
    [UsedImplicitly]
    public class PageHomePageProvider : IHomePageProvider {
        private readonly IPageService _pageService;
        private readonly IPageSlugConstraint _pageSlugConstraint;

        public PageHomePageProvider(IOrchardServices services, IPageService pageService, IPageSlugConstraint pageSlugConstraint) {
            Services = services;
            _pageSlugConstraint = pageSlugConstraint;
            _pageService = pageService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public string GetProviderName() {
            return "PageHomePageProvider";
        }

        public ActionResult GetHomePage(int itemId) {
            var page = _pageService.Get(itemId);
            if (page == null)
                return new NotFoundResult();

            var correctedSlug = _pageSlugConstraint.FindSlug(page.Slug);
            if (correctedSlug == null)
                return new NotFoundResult();

            page = _pageService.Get(correctedSlug);
            if (page == null)
                return new NotFoundResult();

            var model = new PageViewModel {
                Page = Services.ContentManager.BuildDisplayModel(page, "Detail")
            };

            return new ViewResult {
                ViewName = "~/Modules/Orchard.Pages/Views/Page/Item.ascx",
                ViewData = new ViewDataDictionary<PageViewModel>(model)
            };
        }
    }
}
