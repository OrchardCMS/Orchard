using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Pages.Models;
using Orchard.Pages.ViewModels;
using Orchard.Services;

namespace Orchard.Pages.Services {
    public class PagesHomePageProvider : IHomePageProvider {
        private readonly IPageService _pageService;
        private readonly ISlugConstraint _slugConstraint;

        public PagesHomePageProvider(IOrchardServices services, IPageService pageService, ISlugConstraint slugConstraint) {
            Services = services;
            _slugConstraint = slugConstraint;
            _pageService = pageService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        private Localizer T { get; set; }

        #region Implementation of IHomePageProvider

        public string GetProviderName() {
            return "PagesHomePageProvider";
        }

        public ActionResult GetHomePage(int itemId) {
            Page page = _pageService.Get(itemId);
            var correctedSlug = _slugConstraint.LookupPublishedSlug(page.Slug);
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

        #endregion
    }
}
