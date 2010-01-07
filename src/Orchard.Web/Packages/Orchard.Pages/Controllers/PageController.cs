using System;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Pages.Services;
using Orchard.Pages.ViewModels;

namespace Orchard.Pages.Controllers {
    [ValidateInput(false)]
    public class PageController : Controller, IUpdateModel {
        private readonly IPageService _pageService;
        private readonly ISlugConstraint _slugConstraint;

        public PageController(
            IOrchardServices services,
            IPageService pageService,
            ISlugConstraint slugConstraint) {
            Services = services;
            _pageService = pageService;
            _slugConstraint = slugConstraint;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        private Localizer T { get; set; }

        public ActionResult Item(string slug) {
            if (!Services.Authorizer.Authorize(Permissions.ViewPages, T("Couldn't view page")))
                return new HttpUnauthorizedResult();

            if (slug == null) {
                throw new ArgumentNullException("slug");
            }

            //var correctedSlug = _slugConstraint.LookupPublishedSlug(pageSlug);

            var page = _pageService.Get(slug);

            var model = new PageViewModel {
                Page = Services.ContentManager.BuildDisplayModel(page, "Detail")
            };
            return View(model);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}