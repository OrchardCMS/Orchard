using System;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Mvc.Results;
using Orchard.Pages.Services;
using Orchard.Pages.ViewModels;
using Orchard.Security;

namespace Orchard.Pages.Controllers {
    [ValidateInput(false)]
    public class PageController : Controller {
        private readonly IPageService _pageService;
        private readonly ISlugConstraint _slugConstraint;

        public PageController(IOrchardServices services, IPageService pageService, ISlugConstraint slugConstraint) {
            Services = services;
            _pageService = pageService;
            _slugConstraint = slugConstraint;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        private Localizer T { get; set; }

        public ActionResult Item(string slug) {
            if (!Services.Authorizer.Authorize(StandardPermissions.AccessFrontEnd, T("Couldn't view page")))
                return new HttpUnauthorizedResult();

            var correctedSlug = _slugConstraint.LookupPublishedSlug(slug);
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