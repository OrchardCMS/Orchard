using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Results;
using Orchard.Pages.Routing;
using Orchard.Pages.Services;
using Orchard.Pages.ViewModels;
using Orchard.Security;

namespace Orchard.Pages.Controllers {
    [ValidateInput(false)]
    public class PageController : Controller {
        private readonly IPageService _pageService;
        private readonly IPageSlugConstraint _pageSlugConstraint;

        public PageController(IOrchardServices services, IPageService pageService, IPageSlugConstraint pageSlugConstraint) {
            Services = services;
            _pageService = pageService;
            _pageSlugConstraint = pageSlugConstraint;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Item(string slug) {
            if (!Services.Authorizer.Authorize(StandardPermissions.AccessFrontEnd, T("Couldn't view page")))
                return new HttpUnauthorizedResult();

            var correctedSlug = _pageSlugConstraint.FindSlug(slug);
            if (correctedSlug == null)
                return new NotFoundResult();

            var page = _pageService.Get(correctedSlug);
            if (page == null)
                return new NotFoundResult();

            var model = new PageViewModel {
                Page = Services.ContentManager.BuildDisplayModel(page, "Detail")
            };
            return View(model);
        }
    }
}